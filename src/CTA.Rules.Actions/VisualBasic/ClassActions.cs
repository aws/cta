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
    public class ClassActions
    {
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetRemoveBaseClassAction(string baseClass)
        {
            ClassBlockSyntax RemoveBaseClass(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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
        
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAddBaseClassAction(string baseClass)
        {
            ClassBlockSyntax AddBaseClass(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                if (syntaxGenerator != null)
                {
                    node = (ClassBlockSyntax)syntaxGenerator.AddBaseType(node, SyntaxFactory.ParseName(baseClass));
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
        
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetChangeNameAction(string className)
        {
            ClassBlockSyntax ChangeName(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                node = node.WithClassStatement(SyntaxFactory.ClassStatement(SyntaxFactory.Identifier(className)))
                    .NormalizeWhitespace();
                return node;
            }
            return ChangeName;
        }
        
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetRemoveAttributeAction(string attributeName)
        {
            ClassBlockSyntax RemoveAttribute(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                var attributeLists = node.ClassStatement.AttributeLists;
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

                node = node.WithClassStatement(node.ClassStatement.WithAttributeLists(attributeLists))
                    .NormalizeWhitespace();
                return node;
            }

            return RemoveAttribute;
        }
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAddAttributeAction(string attribute)
        {
            ClassBlockSyntax AddAttribute(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                var attributeLists = node.ClassStatement.AttributeLists;
                attributeLists = attributeLists.Add(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName(attribute)))));

                node = node.WithClassStatement(node.ClassStatement.WithAttributeLists(attributeLists))
                    .NormalizeWhitespace();
                return node;
            }
            return AddAttribute;
        }
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAddCommentAction(string comment, string dontUseCTAPrefix = null)
        {
            ClassBlockSyntax AddComment(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                var commentFormat = dontUseCTAPrefix != null ? Constants.VbCommentFormatBlank : Constants.VbCommentFormat;
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.CommentTrivia, string.Format(commentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAddMethodAction(string expression)
        {
            ClassBlockSyntax AddMethod(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                var methodSyntax = SyntaxFactory.ParseSyntaxTree(expression).GetRoot().DescendantNodes()
                    .OfType<MethodStatementSyntax>().FirstOrDefault();
                var methodBlockSyntax = SyntaxFactory.ParseSyntaxTree(expression).GetRoot().DescendantNodes()
                    .OfType<MethodBlockSyntax>().FirstOrDefault();
                if (methodBlockSyntax != null)
                {
                    node = node.AddMembers(methodBlockSyntax.SubOrFunctionStatement);
                    foreach (var statement in methodBlockSyntax.Statements)
                    {
                        node = node.AddMembers(statement);
                    }
                }
                return node.NormalizeWhitespace();
            }
            return AddMethod;
        }
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetRemoveMethodAction(string methodName)
        {
            //TODO  what if there is operator overloading 
            ClassBlockSyntax RemoveMethod(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetRenameClassAction(string newClassName)
        {
            ClassBlockSyntax RenameClass(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                node = node.WithClassStatement(node.ClassStatement.WithIdentifier(SyntaxFactory.Identifier(newClassName))).NormalizeWhitespace();
                return node;
            }
            return RenameClass;
        }
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetReplaceMethodModifiersAction(string methodName, string modifiers)
        {
            ClassBlockSyntax ReplaceMethodModifiers(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                var allMembers = node.Members.ToList();
                var allMethods = allMembers.OfType<MethodStatementSyntax>();
                if (allMethods.Any())
                {
                    var replaceMethod = allMethods.FirstOrDefault(m => m.Identifier.ToString() == methodName);
                    if (replaceMethod != null)
                    {
                        var allModifiersAreValid = modifiers.Split(new char[] { ' ', ',' }).All(m => Constants.SupportedMethodModifiers.Contains(m));
                        if (allModifiersAreValid)
                        {
                            SyntaxTokenList tokenList = new SyntaxTokenList(SyntaxFactory.ParseTokens(modifiers));
                            var newMethod = replaceMethod.WithModifiers(tokenList);

                            node = node.WithMembers(node.Members.Replace(replaceMethod, newMethod)).NormalizeWhitespace();
                        }
                    }
                }

                return node;
            }
            return ReplaceMethodModifiers;
        }
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAddExpressionAction(string expression)
        {
            ClassBlockSyntax AddExpression(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetRemoveConstructorInitializerAction(string initializerArgument)
        {
            ClassBlockSyntax RemoveConstructorInitializer(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAppendConstructorExpressionAction(string expression)
        {
            ClassBlockSyntax AppendConstructorExpression(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetCreateConstructorAction(string types, string identifiers)
        {
            ClassBlockSyntax CreateConstructor(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetChangeMethodNameAction(string existingMethodName, string newMethodName)
        {
            ClassBlockSyntax ChangeMethodName(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetChangeMethodToReturnTaskTypeAction(string methodName)
        {
            ClassBlockSyntax ChangeMethodToReturnTaskType(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetRemoveMethodParametersAction(string methodName)
        {
            ClassBlockSyntax RemoveMethodParameters(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetCommentMethodAction(string methodName, string comment = null, string dontUseCTAPrefix = null)
        {
            ClassBlockSyntax CommentMethod(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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
                    node = node.WithEndClassStatement(node.EndClassStatement.WithLeadingTrivia(trivia));
                }
                return node.NormalizeWhitespace();
            }
            return CommentMethod;
        }

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAddCommentsToMethodAction(string methodName, string comment, string dontUseCTAPrefix = null)
        {
            ClassBlockSyntax AddCommentsToMethod(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAddExpressionToMethodAction(string methodName, string expression)
        {
            ClassBlockSyntax AddExpressionToMethod(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetAddParametersToMethodAction(string methodName, string types, string identifiers)
        {
            ClassBlockSyntax AddParametersToMethod(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
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

        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetReplaceMvcControllerMethodsBodyAction(string expression)
        {
            throw new NotImplementedException();
        }
    
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetReplaceWebApiControllerMethodsBodyAction(string expression)
        {
            ClassBlockSyntax ReplaceMethodBodyFunc(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                return AddCommentToPublicMethods(node, expression);
            }
            return ReplaceMethodBodyFunc;
        }
        public Func<SyntaxGenerator, ClassBlockSyntax, ClassBlockSyntax> GetReplaceCoreControllerMethodsBodyAction(string expression)
        {
            ClassBlockSyntax ReplaceMethodModifiers(SyntaxGenerator syntaxGenerator, ClassBlockSyntax node)
            {
                return AddCommentToPublicMethods(node, expression);
            }
            return ReplaceMethodModifiers;
        }
        
        private ClassBlockSyntax AddCommentToPublicMethods(ClassBlockSyntax node, string expression)
        {
            var comment = $"' Replace method body with {expression}";

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

        private MethodBlockSyntax GetMethodNode(ClassBlockSyntax node, string methodName)
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
