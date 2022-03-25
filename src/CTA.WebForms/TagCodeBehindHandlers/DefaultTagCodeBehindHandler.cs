using System;
using System.Collections.Generic;
using System.Linq;
using CTA.Rules.Config;
using CTA.WebForms.Extensions;
using CTA.WebForms.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.TagCodeBehindHandlers
{
    public class DefaultTagCodeBehindHandler : TagCodeBehindHandler
    {
        private IDictionary<string, string> _generatedBindableProperties;
        private List<MemberDeclarationSyntax> _stagedMemberAdditions;

        /// <summary>
        /// Initializes a new <see cref="TagCodeBehindHandler"/> instance.
        /// </summary>
        /// <param name="codeBehindType">The expected type representation in the code behind for the control that
        /// this handler is tasked with handling.</param>
        /// <param name="idValue">The value of the ID attribute for the control that this handler is tasked with
        /// handling.</param>
        public DefaultTagCodeBehindHandler(string codeBehindType, string idValue) : base(codeBehindType, idValue)
        {
            _generatedBindableProperties = new Dictionary<string, string>();
            _stagedMemberAdditions = new List<MemberDeclarationSyntax>();
        }

        /// <inheritdoc/>
        public override void StageCodeBehindConversionsForAttribute(
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration,
            string codeBehindName,
            string convertedSourceValue)
        {
            if (_generatedBindableProperties.ContainsKey(codeBehindName))
            {
                // We've already handled this attribute, so we can stop here
                // without staging any extra modifications
                return;
            }

            var codeBehindReferences = GetCodeBehindReferencesForAttribute(semanticModel, classDeclaration, codeBehindName);

            if (codeBehindReferences?.Any() ?? false)
            {
                var propertyType = GetMemberType(semanticModel, codeBehindReferences.First());
                var propertyName = GetNewPropertyName(codeBehindName);

                var newProperty = GenerateProperty(propertyType, propertyName);

                if (newProperty == null)
                {
                    LogHelper.LogError($"{Rules.Config.Constants.WebFormsErrorTag}Failed to generate code behind property " +
                        $"for attribute {codeBehindName} on node of type {CodeBehindType} with id {IdValue}");

                    return;
                }

                _generatedBindableProperties.Add(codeBehindName, $"@({propertyName})");

                if (!string.IsNullOrEmpty(convertedSourceValue))
                {
                    newProperty = newProperty.AddComment($"The initial value \"{convertedSourceValue}\" was removed from the " +
                        $"view layer in favor of a binding to the following property");
                }

                _stagedMemberAdditions.Add(newProperty);
                StageCodeBehindReferenceConversions(codeBehindReferences, propertyName);
            }
        }

        /// <inheritdoc/>
        public override void StageCleanUpForUnconvertableReferences(
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration)
        {
            var codeBehindReferences = GetCodeBehindReferencesWithoutStagedConversions(semanticModel, classDeclaration);

            if (codeBehindReferences?.Any() ?? false)
            {
                foreach (var codeBehindReference in codeBehindReferences)
                {
                    var ancestorStatement = GetAncestorStatementSyntax(codeBehindReference);

                    if (ancestorStatement != null)
                    {
                        var comment = CodeSyntaxHelper.GetBlankLine().AddComment(ancestorStatement.ToString(), isLeading: false);
                        StagedConversions.Add((ancestorStatement, comment));
                    }
                    else
                    {
                        LogHelper.LogError($"{Rules.Config.Constants.WebFormsErrorTag}Failed to stage removal of unconvertable " +
                            $"code behind reference with value {codeBehindReference}, ancestor statement syntax could not be found");
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override ClassDeclarationSyntax PerformMemberAdditions(ClassDeclarationSyntax classDeclaration)
        {
            foreach (var memberAddition in _stagedMemberAdditions)
            {
                classDeclaration = classDeclaration.AddMembers(memberAddition);
            }

            return classDeclaration;
        }

        /// <inheritdoc/>
        public override string GetBindingIfExists(string codeBehindName, string targetAttribute)
        {
            if (_generatedBindableProperties.ContainsKey(codeBehindName))
            {
                return string.IsNullOrEmpty(targetAttribute)
                    ? _generatedBindableProperties[codeBehindName]
                    : $"{targetAttribute}=\"{_generatedBindableProperties[codeBehindName]}\"";
            }

            return null;
        }

        /// <summary>
        /// Retrieves the nearest ancestor node of <paramref name="expressionSyntax"/> that is
        /// of type <see cref="StatementSyntax"/>.
        /// </summary>
        /// <param name="expressionSyntax">The expression whose ancestor statement is being searched for.</param>
        /// <returns>The nearest ancestor node of type <see cref="StatementSyntax"/> or null on failure.</returns>
        private StatementSyntax GetAncestorStatementSyntax(ExpressionSyntax expressionSyntax)
        {
            SyntaxNode current = expressionSyntax;

            while (current != null && !(current is StatementSyntax)) {
                current = current.Parent;
            }

            return current as StatementSyntax;
        }

        /// <summary>
        /// Stages replacement conversions for the provided code behind references with a new identifier.
        /// </summary>
        /// <param name="codeBehindReferences">The code behind references to comment out.</param>
        /// <param name="newPropertyName">The name to use on the replacement identifier.</param>
        private void StageCodeBehindReferenceConversions(
            IEnumerable<MemberAccessExpressionSyntax> codeBehindReferences,
            string newPropertyName)
        {
            foreach (var codeBehindReference in codeBehindReferences)
            {
                var propertyIdentifierSyntax = SyntaxFactory.IdentifierName(newPropertyName);

                StagedConversions.Add((codeBehindReference, propertyIdentifierSyntax));
            }
        }

        /// <summary>
        /// Generates a member declaration for a property of type <paramref name="propertyType"/> with name
        /// <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyType">The type to make the generated property.</param>
        /// <param name="propertyName">The name to give the generated property.</param>
        /// <returns>A member declaration containing the new property.</returns>
        private MemberDeclarationSyntax GenerateProperty(string propertyType, string propertyName)
        {
            if (propertyType == null || propertyName == null)
            {
                return null;
            }

            return SyntaxFactory.ParseMemberDeclaration($"public {propertyType} {propertyName} {{ get; set; }}");
        }

        /// <summary>
        /// Retrieves the name of the type that a given member is.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="memberAccessExpression"/>
        /// belongs to.</param>
        /// <param name="memberAccessExpression">The expression containing the member whose type is to be
        /// evaluated.</param>
        /// <returns>The name of the type that the member being accessed in
        /// <paramref name="memberAccessExpression"/> is.</returns>
        private string GetMemberType(
            SemanticModel semanticModel,
            MemberAccessExpressionSyntax memberAccessExpression)
        {
            try
            {
                var typeInfo = semanticModel.GetTypeInfo(memberAccessExpression.Name);

                return typeInfo.Type?.Name;
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to assess type of " +
                    $"expression {memberAccessExpression}");

                return null;
            }
        }

        /// <summary>
        /// Gets the name of the property to be generated for a given node attribute.
        /// </summary>
        /// <param name="codeBehindName">The name of the attribute currently being converted as it will appear in
        /// the code behind.</param>
        /// <returns>The name to be used when generating a property for <paramref name="codeBehindName"/>.</returns>
        private string GetNewPropertyName(string codeBehindName)
        {
            return $"{IdValue}_{codeBehindName}";
        }

        /// <summary>
        /// Gets code behind references to the node that the handler is responsible for.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="classDeclaration"/> belongs to.</param>
        /// <param name="classDeclaration">The declaration syntax for the code behind being converted.</param>
        /// <returns>The set of discovered references to the node that the handler is responsible for.</returns>
        private IEnumerable<MemberAccessExpressionSyntax> GetCodeBehindReferences(
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration
                .DescendantNodes()
                .Select(node => node as MemberAccessExpressionSyntax)
                .Where(node => node != null && ExpressionIsId(semanticModel, node.Expression));
        }

        /// <summary>
        /// Gets code behind references to the specified attribute of the node that the handler is responsible for.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="classDeclaration"/> belongs to.</param>
        /// <param name="classDeclaration">The declaration syntax for the code behind being converted.</param>
        /// <param name="codeBehindName">The name of the attribute currently being converted as it will appear in
        /// the code behind.</param>
        /// <returns>The set of discovered references to the specified attribute of the node that the handler is
        /// responsible for.</returns>
        private IEnumerable<MemberAccessExpressionSyntax> GetCodeBehindReferencesForAttribute(
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration,
            string codeBehindName)
        {
            return GetCodeBehindReferences(semanticModel, classDeclaration)
                .Where(node => NameIsAttribute(node.Name, codeBehindName));
        }

        /// <summary>
        /// Gets code behind references for the node the handler is responsible for which have not yet had any
        /// changes staged for them.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="classDeclaration"/> belongs to.</param>
        /// <param name="classDeclaration">The declaration syntax for the code behind being converted.</param>
        /// <returns>The set of discovered code behind references with no staged conversions.</returns>
        private IEnumerable<MemberAccessExpressionSyntax> GetCodeBehindReferencesWithoutStagedConversions(
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration)
        {
            return GetCodeBehindReferences(semanticModel, classDeclaration)
                .Where(node => !_generatedBindableProperties.ContainsKey(node.Name.Identifier.ValueText));
        }

        /// <summary>
        /// Checks that a given expression corresponds to a valid representation of the identifier of the control
        /// that the handler is responsible for.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="expressionSyntax"/> belongs to.</param>
        /// <param name="expressionSyntax">The expression that is being evaluated.</param>
        /// <returns><c>true</c> if the expression has the correct type and name, <c>false</c> otherwise.</returns>
        private bool ExpressionIsId(
            SemanticModel semanticModel,
            ExpressionSyntax expressionSyntax)
        {
            return ExpressionIsOnlyId(semanticModel, expressionSyntax)
                || ExpressionIsThisDotId(semanticModel, expressionSyntax);
        }

        /// <summary>
        /// Checks that a given expression corresponds to only the identifier of the control that the handler
        /// is responsible for.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="expressionSyntax"/> belongs to.</param>
        /// <param name="expressionSyntax">The expression that is being evaluated.</param>
        /// <returns><c>true</c> if the expression has the correct type and name, <c>false</c> otherwise.</returns>
        private bool ExpressionIsOnlyId(
            SemanticModel semanticModel,
            ExpressionSyntax expressionSyntax)
        {
            var asIdentifier = expressionSyntax as IdentifierNameSyntax;

            if (asIdentifier != null && asIdentifier.Identifier.ValueText.Equals(IdValue))
            {
                try
                {
                    var typeInfoType = semanticModel.GetTypeInfo(asIdentifier).Type;

                    if (typeInfoType == null)
                    {
                        return false;
                    }

                    var typeFullName = $"{typeInfoType.ContainingNamespace}.{typeInfoType.Name}";

                    return typeFullName.Equals(CodeBehindType);
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to check whether expression " +
                        $"corresponds to code behind reference of type {CodeBehindType} with name {IdValue}, error during " +
                        $"type assessment of expression {expressionSyntax}");
                }
            }

            return false;
        }

        /// <summary>
        /// Checks that a given expression corresponds to "this." + the identifier of the control that the handler
        /// is responsible for.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="expressionSyntax"/> belongs to.</param>
        /// <param name="expressionSyntax">The expression that is being evaluated.</param>
        /// <returns><c>true</c> if the expression has the correct type and name, <c>false</c> otherwise.</returns>
        private bool ExpressionIsThisDotId(
            SemanticModel semanticModel,
            ExpressionSyntax expressionSyntax)
        {
            var asMemberAccess = expressionSyntax as MemberAccessExpressionSyntax;

            return asMemberAccess != null
                && asMemberAccess.Expression is ThisExpressionSyntax
                && ExpressionIsOnlyId(semanticModel, asMemberAccess.Name);
        }

        /// <summary>
        /// Checks that a given name syntax has a given value.
        /// </summary>
        /// <param name="simpleName">The name syntax to be evaluated.</param>
        /// <param name="codeBehindName">The value to compare <paramref name="simpleName"/> against.</param>
        /// <returns><c>true</c> if <paramref name="simpleName"/> has the value of <paramref name="codeBehindName"/>,
        /// <c>false</c> otherwise.</returns>
        private bool NameIsAttribute(
            SimpleNameSyntax simpleName,
            string codeBehindName)
        {
            var asIdentifier = simpleName as IdentifierNameSyntax;

            return asIdentifier != null && asIdentifier.Identifier.ValueText.Equals(codeBehindName);
        }
    }
}
