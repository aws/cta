using System;
using System.Collections.Generic;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.VisualBasic
{
    /// <summary>
    /// List of actions that can run on Class Blocks
    /// </summary>
    public class TypeBlockActions
    {
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetRemoveBaseClassAction(string baseClass)
        {
            TypeBlockSyntax RemoveBaseClass(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var currentBaseTypes = node.Inherits.FirstOrDefault()?.Types ?? new SeparatedSyntaxList<TypeSyntax>();
                SeparatedSyntaxList<TypeSyntax> newBaseTypes = new SeparatedSyntaxList<TypeSyntax>();

                foreach (var baseTypeSyntax in currentBaseTypes)
                {
                    if (!baseTypeSyntax.GetText().ToString().Trim().Equals(baseClass))
                    {
                        newBaseTypes.Add(baseTypeSyntax);
                    }
                }
                
                node = node.WithInherits(new SyntaxList<InheritsStatementSyntax>
                {
                    SyntaxFactory.InheritsStatement().WithTypes(newBaseTypes)
                });
            
                return node;
            }

            return RemoveBaseClass;
        }
        
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAddBaseClassAction(string baseClass)
        {
            TypeBlockSyntax AddBaseClass(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                if (syntaxGenerator != null)
                {
                    node = (TypeBlockSyntax)syntaxGenerator.AddBaseType(node, SyntaxFactory.ParseName(baseClass));
                }
                else
                {
                    var baseType = SyntaxFactory.InheritsStatement(SyntaxFactory.ParseTypeName(baseClass));
                    node = node.WithInherits(new SyntaxList<InheritsStatementSyntax>(baseType));
                }
                return node;
            }
            return AddBaseClass;
        }
        
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetChangeNameAction(string className)
        {
            TypeBlockSyntax ChangeName(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                node = node.WithBlockStatement(node.BlockStatement.WithIdentifier(SyntaxFactory.Identifier(className)))
                    .NormalizeWhitespace();
                return node;
            }
            return ChangeName;
        }
        
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetRemoveAttributeAction(string attributeName)
        {
            TypeBlockSyntax RemoveAttribute(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var attributeLists = node.BlockStatement.AttributeLists;
                AttributeListSyntax attributeToRemove = null;

                foreach (var attributeList in attributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (attribute.Name.ToString() == attributeName)
                        {
                            attributeToRemove = attributeList;
                            break;
                        }
                    }
                }

                if (attributeToRemove != null)
                {
                    attributeLists = attributeLists.Remove(attributeToRemove);
                }

                node = node.WithBlockStatement(node.BlockStatement.WithAttributeLists(attributeLists))
                    .NormalizeWhitespace();
                return node;
            }

            return RemoveAttribute;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAddAttributeAction(string attribute)
        {
            TypeBlockSyntax AddAttribute(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var attributeLists = node.BlockStatement.AttributeLists;
                attributeLists = attributeLists.Add(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName(attribute)))));

                node = node.WithBlockStatement(node.BlockStatement.WithAttributeLists(attributeLists))
                    .NormalizeWhitespace();
                return node;
            }
            return AddAttribute;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAddCommentAction(string comment, string dontUseCTAPrefix = null)
        {
            TypeBlockSyntax AddComment(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                var commentFormat = dontUseCTAPrefix != null ? Constants.VbCommentFormatBlank : Constants.VbCommentFormat;
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.CommentTrivia, string.Format(commentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAddMethodAction(string expression)
        {
            TypeBlockSyntax AddMethod(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var methodBlockSyntax = SyntaxFactory.ParseSyntaxTree(expression).GetRoot().DescendantNodes()
                    .OfType<MethodBlockSyntax>().FirstOrDefault();
                if (methodBlockSyntax != null)
                {
                    node = node.AddMembers(methodBlockSyntax);
                }
                return node.NormalizeWhitespace();
            }
            return AddMethod;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetRemoveMethodAction(string methodName)
        {
            //TODO  what if there is operator overloading 
            TypeBlockSyntax RemoveMethod(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var allMembers = node.Members.ToList();
                var allMethods = allMembers.OfType<MethodStatementSyntax>();
                if (allMethods.Any())
                {
                    var removeMethod = allMethods.FirstOrDefault(m => m.Identifier.ToString() == methodName);
                    if (removeMethod != null)
                    {
                        node = node.RemoveNode(removeMethod, SyntaxRemoveOptions.KeepNoTrivia).NormalizeWhitespace();
                    }
                }

                return node;
            }
            return RemoveMethod;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetRenameClassAction(string newClassName)
        {
            TypeBlockSyntax RenameClass(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                node = node.WithBlockStatement(node.BlockStatement.WithIdentifier(SyntaxFactory.Identifier(newClassName))).NormalizeWhitespace();
                return node;
            }
            return RenameClass;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetReplaceMethodModifiersAction(string methodName, string modifiers)
        {
            TypeBlockSyntax ReplaceMethodModifiers(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var allMethods = node.Members.OfType<MethodStatementSyntax>();
                if (allMethods.Any())
                {
                    var replaceMethod = allMethods.FirstOrDefault(m => m.Identifier.ToString() == methodName);
                    if (replaceMethod != null)
                    {
                        var allModifiers = modifiers.Split(new char[] { ' ', ',' });
                        if (allModifiers.All(m => Constants.SupportedVbMethodModifiers.Contains(m)))
                        {
                            SyntaxTokenList tokenList = new SyntaxTokenList();
                            foreach (string m in allModifiers)
                            {
                                if (m == "Async")
                                {
                                    // for some reason syntax factory can't parse that async is a keyword
                                    tokenList = tokenList.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
                                }
                                else
                                {
                                    tokenList = tokenList.Add(SyntaxFactory.ParseToken(m));
                                }
                            }
                            var newMethod = replaceMethod.WithModifiers(tokenList);

                            node = node.WithMembers(node.Members.Replace(replaceMethod, newMethod)).NormalizeWhitespace();
                        }
                    }
                }

                return node;
            }
            return ReplaceMethodModifiers;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAddExpressionAction(string expression)
        {
            TypeBlockSyntax AddExpression(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var parsedExpression = SyntaxFactory.ParseExecutableStatement(expression);
                if (!parsedExpression.FullSpan.IsEmpty)
                {
                    var nodeDeclarations = node.Members;
                    nodeDeclarations = nodeDeclarations.Insert(0, parsedExpression);
                    node = node.WithMembers(nodeDeclarations).NormalizeWhitespace();
                }
                return node;
            }
            return AddExpression;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetRemoveConstructorInitializerAction(string initializerArgument)
        {
            TypeBlockSyntax RemoveConstructorInitializer(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var constructor = node.ChildNodes().FirstOrDefault(c => c.IsKind(SyntaxKind.ConstructorBlock));
                if (constructor != null)
                {
                    var constructorNode = (ConstructorBlockSyntax)constructor;
                    var newArguments = new SeparatedSyntaxList<ArgumentSyntax>();
                    // base initializers should be the first statement
                    var firstStatement = constructorNode.Statements.FirstOrDefault();
                    if (firstStatement != null &&
                        firstStatement.DescendantNodes().Any(s => s.IsKind(SyntaxKind.MyBaseExpression)))
                    {
                        var arguments = firstStatement.DescendantNodes().OfType<ArgumentListSyntax>()
                            .FirstOrDefault();
                        if (arguments != null)
                        {
                            foreach (var arg in arguments.Arguments)
                            {
                                if (!arg.GetText().ToString().Trim().Equals(initializerArgument))
                                {
                                    newArguments = newArguments.Add(arg);
                                }
                            }
                            
                            if (newArguments.Any())
                            {
                                node = node.ReplaceNode(arguments, SyntaxFactory.ArgumentList(newArguments))
                                    .NormalizeWhitespace();
                            }
                        }
                    }
                }
                return node;
            }

            return RemoveConstructorInitializer;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAppendConstructorExpressionAction(string expression)
        {
            TypeBlockSyntax AppendConstructorExpression(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                var constructor = node.Members.FirstOrDefault(c => c.IsKind(SyntaxKind.ConstructorBlock));
                if (constructor != null)
                {
                    ConstructorBlockSyntax constructorNode = (ConstructorBlockSyntax)constructor;
                    StatementSyntax statementExpression = SyntaxFactory.ParseExecutableStatement(expression);
                    if (!statementExpression.FullSpan.IsEmpty)
                    {
                        constructorNode = constructorNode.AddStatements(statementExpression);
                        node = node.ReplaceNode(constructor, constructorNode).NormalizeWhitespace();
                    }
                }
                return node;
            }
            return AppendConstructorExpression;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetCreateConstructorAction(string types, string identifiers)
        {
            TypeBlockSyntax CreateConstructor(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                // constructors in vb are just named new
                var constructorStatementNode = SyntaxFactory.SubNewStatement()
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                // Add constructor parameters if provided
                if (!string.IsNullOrWhiteSpace(identifiers) && !string.IsNullOrWhiteSpace(types))
                {
                    var identifiersArray = identifiers.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var typesArray = types.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (identifiersArray.Length == typesArray.Length)
                    {
                        List<ParameterSyntax> parameters = new List<ParameterSyntax>();
                        for (var i = 0; i < identifiersArray.Length; i++)
                        {
                            parameters.Add(SyntaxFactory
                                .Parameter(SyntaxFactory.ModifiedIdentifier(identifiersArray[i]))
                                .WithAsClause(SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName(typesArray[i]))));
                        }

                        constructorStatementNode = constructorStatementNode.AddParameterListParameters(parameters.ToArray());
                    }
                }

                var constructorBlock = SyntaxFactory.ConstructorBlock(constructorStatementNode);
                node = node.AddMembers(constructorBlock).NormalizeWhitespace();
                
                return node;
            }
            return CreateConstructor;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetChangeMethodNameAction(string existingMethodName, string newMethodName)
        {
            TypeBlockSyntax ChangeMethodName(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, existingMethodName);
                if (methodNode != null)
                {
                    var methodActions = new MethodBlockActions();
                    var changeMethodNameFunc = methodActions.GetChangeMethodNameAction(newMethodName);
                    var newMethodNode = changeMethodNameFunc(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return ChangeMethodName;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetChangeMethodToReturnTaskTypeAction(string methodName)
        {
            TypeBlockSyntax ChangeMethodToReturnTaskType(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    var methodActions = new MethodBlockActions();
                    var changeMethodToReturnTaskTypeActionFunc = methodActions.GetChangeMethodToReturnTaskTypeAction();
                    var newMethodNode = changeMethodToReturnTaskTypeActionFunc(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return ChangeMethodToReturnTaskType;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetRemoveMethodParametersAction(string methodName)
        {
            TypeBlockSyntax RemoveMethodParameters(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    var parameters = methodNode.SubOrFunctionStatement.ParameterList.Parameters;
                    MethodBlockActions methodActions = new MethodBlockActions();
                    var removeMethodParametersActionFunc = methodActions.GetRemoveMethodParametersAction();
                    var newMethodNode = removeMethodParametersActionFunc(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node.NormalizeWhitespace();
            }
            return RemoveMethodParameters;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetCommentMethodAction(string methodName, string comment = null, string dontUseCTAPrefix = null)
        {
            TypeBlockSyntax CommentMethod(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);

                if (methodNode != null)
                {
                    var methodActions = new MethodBlockActions();
                    var commentMethodAction = methodActions.GetCommentMethodAction(comment, dontUseCTAPrefix);
                    var newMethodNode = commentMethodAction(syntaxGenerator, methodNode);

                    var methodStatementComment =
                        SyntaxFactory.CommentTrivia($"' {newMethodNode.SubOrFunctionStatement.ToFullString()}");
                    var methodBodyComment = newMethodNode.EndSubOrFunctionStatement.GetLeadingTrivia();
                    var methodEndStatementComment =
                        SyntaxFactory.CommentTrivia($"' {newMethodNode.EndSubOrFunctionStatement.ToString()}");

                    var trivia = new SyntaxTriviaList();
                    trivia = trivia.Add(methodStatementComment);
                    trivia = trivia.AddRange(methodBodyComment);
                    trivia = trivia.Add(methodEndStatementComment);

                    node = node.RemoveNode(methodNode, SyntaxRemoveOptions.KeepNoTrivia);
                    node = node.WithEndBlockStatement(node.EndBlockStatement.WithLeadingTrivia(trivia));
                }
                return node.NormalizeWhitespace();
            }
            return CommentMethod;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAddCommentsToMethodAction(string methodName, string comment, string dontUseCTAPrefix = null)
        {
            TypeBlockSyntax AddCommentsToMethod(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        var methodActions = new MethodBlockActions();
                        var addCommentActionFunc = methodActions.GetAddCommentAction(comment, dontUseCTAPrefix);
                        var newMethodNode = addCommentActionFunc(syntaxGenerator, methodNode);
                        node = node.ReplaceNode(methodNode, newMethodNode);
                    }
                }
                return node;
            }
            return AddCommentsToMethod;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAddExpressionToMethodAction(string methodName, string expression)
        {
            TypeBlockSyntax AddExpressionToMethod(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    var methodActions = new MethodBlockActions();
                    var addExpressionToMethodAction = methodActions.GetAddExpressionToMethodAction(expression);
                    var newMethodNode = addExpressionToMethodAction(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return AddExpressionToMethod;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetAddParametersToMethodAction(string methodName, string types, string identifiers)
        {
            TypeBlockSyntax AddParametersToMethod(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    var methodActions = new MethodBlockActions();
                    var addParametersToMethodAction = methodActions.GetAddParametersToMethodAction(types, identifiers);
                    var newMethodNode = addParametersToMethodAction(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode).NormalizeWhitespace();
                }
                return node;
            }
            return AddParametersToMethod;
        }

        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetReplaceMvcControllerMethodsBodyAction(string expression)
        {
            TypeBlockSyntax ReplaceMvcControllerMethodsBodyFunc(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                return node;
            }

            return ReplaceMvcControllerMethodsBodyFunc;
        }
    
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetReplaceWebApiControllerMethodsBodyAction(string expression)
        {
            TypeBlockSyntax ReplaceMethodBodyFunc(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                return AddCommentToPublicMethods(node, expression);
            }
            return ReplaceMethodBodyFunc;
        }
        public Func<SyntaxGenerator, TypeBlockSyntax, TypeBlockSyntax> GetReplaceCoreControllerMethodsBodyAction(string expression)
        {
            TypeBlockSyntax ReplaceCoreControllerMethodsBody(SyntaxGenerator syntaxGenerator, TypeBlockSyntax node)
            {
                return AddCommentToPublicMethods(node, expression);
            }
            return ReplaceCoreControllerMethodsBody;
        }
        
        private TypeBlockSyntax AddCommentToPublicMethods(TypeBlockSyntax node, string expression)
        {
            var comment = string.Format(Constants.VbCommentFormat, $"Replace method body with {expression}");

            var allMembers = node.Members.ToList();
            var allMethods = allMembers.OfType<MethodBlockSyntax>()
                .Where(m => m.SubOrFunctionStatement.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                .Select(mb => GetMethodId(mb.SubOrFunctionStatement)).ToList();

            foreach (var method in allMethods)
            {
                var currentMethodStatement = node.DescendantNodes().OfType<MethodStatementSyntax>()
                    .FirstOrDefault(m => GetMethodId(m) == method);
                var originalMethod = currentMethodStatement;
                if (currentMethodStatement != null)
                {
                    var trivia = currentMethodStatement.GetLeadingTrivia();
                    trivia = trivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.CommentTrivia, comment));
                    currentMethodStatement = currentMethodStatement.WithLeadingTrivia(trivia).NormalizeWhitespace();
                    node = node.ReplaceNode(originalMethod, currentMethodStatement);
                }
            }
            return node;
        }

        private string GetMethodId(MethodStatementSyntax method)
        {
            return $"{method.Identifier}{method.ParameterList}";
        }

        private MethodBlockSyntax GetMethodNode(TypeBlockSyntax node, string methodName)
        {
            var methodNodeList = node.DescendantNodes().OfType<MethodBlockSyntax>()
                .Where(method => method.SubOrFunctionStatement.Identifier.Text == methodName);
            if (methodNodeList != null && methodNodeList.Count() > 1)
            {
                return null;
            }
            return methodNodeList.FirstOrDefault();
        }
    }
}
