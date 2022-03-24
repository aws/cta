using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.TagCodeBehindHandlers
{
    public abstract class TagCodeBehindHandler
    {
        /// <summary>
        /// The expected type representation in the code behind for the control that
        /// this handler is tasked with handling.
        /// </summary>
        public string CodeBehindType { get; }
        /// <summary>
        /// The value of the ID attribute for the control that this handler is tasked with
        /// handling.
        /// </summary>
        public string IdValue { get; }
        /// <summary>
        /// Conversions that have been staged for code behind references to the control
        /// that this handler is tasked with handling.
        /// </summary>
        public IDictionary<SyntaxNode, SyntaxNode> StagedConversions;

        /// <summary>
        /// Initializes a new <see cref="TagCodeBehindHandler"/> instance.
        /// </summary>
        /// <param name="codeBehindType">The expected type representation in the code behind for the control that
        /// this handler is tasked with handling.</param>
        /// <param name="idValue">The value of the ID attribute for the control that this handler is tasked with
        /// handling.</param>
        public TagCodeBehindHandler(string codeBehindType, string idValue)
        {
            CodeBehindType = codeBehindType;
            IdValue = idValue;
            StagedConversions = new Dictionary<SyntaxNode, SyntaxNode>();
        }

        /// <summary>
        /// Stages conversions of any references to the attribute named <paramref name="codeBehindName"/> to a
        /// new represenation as a generated bindable property.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="classDeclaration"/> belongs to.</param>
        /// <param name="classDeclaration">The declaration syntax for the code behind being converted.</param>
        /// <param name="codeBehindName">The name of the attribute currently being converted as it will appear in
        /// the code behind.</param>
        /// <param name="convertedSourceValue">The converted source attribute value, should no
        /// code behind conversions be necessary.</param>
        public abstract void StageCodeBehindConversionsForAttribute(
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration,
            string codeBehindName,
            string convertedSourceValue);

        /// <summary>
        /// Stages removal operations for any unconvertable references to code behind attributes for this
        /// handler's node and does any other code behind reference related clean up.
        /// </summary>
        /// <param name="semanticModel">The semantic model that <paramref name="classDeclaration"/> belongs to.</param>
        /// <param name="classDeclaration">The declaration syntax for the code behind being converted.</param>
        public abstract void StageCleanUpForUnconvertableReferences(
            SemanticModel semanticModel,
            ClassDeclarationSyntax classDeclaration);

        /// <summary>
        /// Adds any members staged for addition to the code behind.
        /// </summary>
        /// <param name="classDeclaration">The declaration syntax for the code behind being converted.</param>
        /// <returns>The modified version of <paramref name="classDeclaration"/>.</returns>
        public abstract ClassDeclarationSyntax PerformMemberAdditions(ClassDeclarationSyntax classDeclaration);

        /// <summary>
        /// Generates replacement text for an attribute assignment or
        /// a basic replacement, using a binding to a generated code behind
        /// property if one exists.
        /// </summary>
        /// <param name="codeBehindName">The name of the attribute being converted as it will
        /// appear in the code behind.</param>
        /// <param name="targetAttribute">The attribute that the conversion result will be assigned to,
        /// if one exists, otherwise null.</param>
        /// <returns>Replacement text for placeholder using a binding to a generated
        /// code behind property if one exists, otherwise null.</returns>
        public abstract string GetBindingIfExists(string codeBehindName, string targetAttribute);
    }
}
