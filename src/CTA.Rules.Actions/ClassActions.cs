using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on Class Declarations
    /// </summary>
    public class ClassActions
    {
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRemoveBaseClassAction(string baseClass)
        {
            ClassDeclarationSyntax RemoveBaseClass(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                SeparatedSyntaxList<BaseTypeSyntax> currentBaseTypes = node.BaseList.Types;
                SeparatedSyntaxList<BaseTypeSyntax> newBaseTypes = new SeparatedSyntaxList<BaseTypeSyntax>();

                foreach (var baseTypeSyntax in currentBaseTypes)
                {
                    if (!baseTypeSyntax.GetText().ToString().Trim().Equals(baseClass))
                    {
                        newBaseTypes.Add(baseTypeSyntax);
                    }
                }

                if (!newBaseTypes.Any())
                {
                    node = node.WithBaseList(null);
                }
                else
                {
                    node = node.WithBaseList(node.BaseList.WithTypes(newBaseTypes));
                }
                return node;
            }

            return RemoveBaseClass;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddBaseClassAction(string baseClass)
        {
            ClassDeclarationSyntax AddBaseClass(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                if (syntaxGenerator != null)
                {
                    node = (ClassDeclarationSyntax)syntaxGenerator.AddBaseType(node, SyntaxFactory.ParseName(baseClass));
                }
                else
                {
                    var baseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(baseClass));
                    node = node.AddBaseListTypes(baseType);
                }
                return node;
            }
            return AddBaseClass;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetChangeNameAction(string className)
        {
            ClassDeclarationSyntax ChangeName(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                node = node.WithIdentifier(SyntaxFactory.Identifier(className)).NormalizeWhitespace();
                return node;
            }
            return ChangeName;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRemoveAttributeAction(string attributeName)
        {
            ClassDeclarationSyntax RemoveAttribute(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var attributeLists = node.AttributeLists;
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

                node = node.WithAttributeLists(attributeLists).NormalizeWhitespace();
                return node;
            }

            return RemoveAttribute;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddAttributeAction(string attribute)
        {
            ClassDeclarationSyntax AddAttribute(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var attributeLists = node.AttributeLists;
                attributeLists = attributeLists.Add(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName(attribute)))));

                node = node.WithAttributeLists(attributeLists).NormalizeWhitespace();
                return node;
            }
            return AddAttribute;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddCommentAction(string comment)
        {
            ClassDeclarationSyntax AddComment(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                //TODO see if this will lead NPE    
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddMethodAction(string expression)
        {
            ClassDeclarationSyntax AddMethod(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var allMembers = node.Members;
                allMembers = allMembers.Add(SyntaxFactory.ParseMemberDeclaration(expression));
                node = node.WithMembers(allMembers).NormalizeWhitespace();
                return node;
            }
            return AddMethod;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRemoveMethodAction(string methodName)
        {
            //TODO  what if there is operator overloading 
            ClassDeclarationSyntax RemoveMethod(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var allMembers = node.Members.ToList();
                var allMethods = allMembers.OfType<MethodDeclarationSyntax>();
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
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRenameClassAction(string newClassName)
        {
            ClassDeclarationSyntax RenameClass(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                node = node.WithIdentifier(SyntaxFactory.Identifier(newClassName)).NormalizeWhitespace();
                return node;
            }
            return RenameClass;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetReplaceMethodModifiersAction(string methodName, string modifiers)
        {
            ClassDeclarationSyntax ReplaceMethodModifiers(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var allMembers = node.Members.ToList();
                var allMethods = allMembers.OfType<MethodDeclarationSyntax>();
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
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddExpressionAction(string expression)
        {
            ClassDeclarationSyntax AddExpression(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                MemberDeclarationSyntax parsedExpression = SyntaxFactory.ParseMemberDeclaration(expression);
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
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRemoveConstructorInitializerAction(string baseClass)
        {
            ClassDeclarationSyntax RemoveConstructorInitializer(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var constructor = node.ChildNodes().Where(c => Microsoft.CodeAnalysis.CSharp.CSharpExtensions.Kind(c) == SyntaxKind.ConstructorDeclaration).FirstOrDefault();
                if (constructor != null)
                {
                    ConstructorDeclarationSyntax constructorNode = (ConstructorDeclarationSyntax)constructor;

                    SeparatedSyntaxList<ArgumentSyntax> initializerArguments = constructorNode.Initializer.ArgumentList.Arguments;
                    SeparatedSyntaxList<ArgumentSyntax> newArguments = new SeparatedSyntaxList<ArgumentSyntax>();

                    foreach (var argument in initializerArguments)
                    {
                        if (!argument.GetText().ToString().Trim().Equals(baseClass))
                        {
                            newArguments = newArguments.Add(argument);
                        }
                    }

                    if (!newArguments.Any())
                    {
                        constructorNode = constructorNode.WithInitializer(null);
                    }
                    else
                    {
                        constructorNode = constructorNode.WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer).AddArgumentListArguments(newArguments.ToArray()));
                    }
                    node = node.ReplaceNode(constructor, constructorNode).NormalizeWhitespace();
                }
                return node;
            }

            return RemoveConstructorInitializer;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAppendConstructorExpressionAction(string expression)
        {
            ClassDeclarationSyntax AppendConstructorExpression(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var constructor = node.Members.Where(c => Microsoft.CodeAnalysis.CSharp.CSharpExtensions.Kind(c) == SyntaxKind.ConstructorDeclaration).FirstOrDefault();
                if (constructor != null)
                {
                    ConstructorDeclarationSyntax constructorNode = (ConstructorDeclarationSyntax)constructor;
                    StatementSyntax statementExpression = SyntaxFactory.ParseStatement(expression);
                    if (!statementExpression.FullSpan.IsEmpty)
                    {
                        constructorNode = constructorNode.AddBodyStatements(statementExpression);
                        node = node.ReplaceNode(constructor, constructorNode).NormalizeWhitespace();
                    }
                }
                return node;
            }
            return AppendConstructorExpression;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetCreateConstructorAction(string types, string identifiers)
        {

            ClassDeclarationSyntax CreateConstructor(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var constructorName = node.Identifier.Value.ToString();
                if (!string.IsNullOrWhiteSpace(constructorName))
                {
                    var constructorNode = SyntaxFactory.ConstructorDeclaration(constructorName).AddBodyStatements().AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                    // Add constructor parameters if provided
                    if (!string.IsNullOrWhiteSpace(identifiers) && !string.IsNullOrWhiteSpace(types))
                    {
                        var identifiersArray = identifiers.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        var typesArray = types.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        if (identifiersArray.Length == typesArray.Length)
                        {
                            List<ParameterSyntax> parameters = new List<ParameterSyntax>();
                            for (int i = 0; i < identifiersArray.Length; i++)
                            {
                                parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(identifiersArray[i])).WithType(SyntaxFactory.ParseTypeName(typesArray[i])));
                            }
                            constructorNode = constructorNode.AddParameterListParameters(parameters.ToArray());
                        }

                    };
                    node = node.AddMembers(constructorNode).NormalizeWhitespace();
                }
                return node;
            }
            return CreateConstructor;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetChangeMethodNameAction(string existingMethodName, string newMethodName)
        {
            ClassDeclarationSyntax ChangeMethodName(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, existingMethodName);
                if (methodNode != null)
                {
                    MethodDeclarationActions methodActions = new MethodDeclarationActions();
                    var changeMethodNameFunc = methodActions.GetChangeMethodNameAction(newMethodName);
                    var newMethodNode = changeMethodNameFunc(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return ChangeMethodName;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetChangeMethodToReturnTaskTypeAction(string methodName)
        {
            ClassDeclarationSyntax ChangeMethodToReturnTaskType(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    MethodDeclarationActions methodActions = new MethodDeclarationActions();
                    var changeMethodToReturnTaskTypeActionFunc = methodActions.GetChangeMethodToReturnTaskTypeAction(methodName);
                    var newMethodNode = changeMethodToReturnTaskTypeActionFunc(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return ChangeMethodToReturnTaskType;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRemoveMethodParametersAction(string methodName)
        {
            ClassDeclarationSyntax RemoveMethodParameters(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    MethodDeclarationActions methodActions = new MethodDeclarationActions();
                    var removeMethodParametersActionFunc = methodActions.GetRemoveMethodParametersAction();
                    var newMethodNode = removeMethodParametersActionFunc(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return RemoveMethodParameters;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetCommentMethodAction(string methodName, string comment = null)
        {
            ClassDeclarationSyntax CommentMethod(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);

                if (methodNode != null)
                {
                    MethodDeclarationActions methodActions = new MethodDeclarationActions();
                    var commentMethodAction = methodActions.GetCommentMethodAction(comment);
                    var newMethodNode = commentMethodAction(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return CommentMethod;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddCommentsToMethodAction(string methodName, string comment)
        {
            ClassDeclarationSyntax AddCommentsToMethod(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        MethodDeclarationActions methodActions = new MethodDeclarationActions();
                        var addCommentActionFunc = methodActions.GetAddCommentAction(comment);
                        var newMethodNode = addCommentActionFunc(syntaxGenerator, methodNode);
                        node = node.ReplaceNode(methodNode, newMethodNode);
                    }
                }
                return node;
            }
            return AddCommentsToMethod;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddExpressionToMethodAction(string methodName, string expression)
        {
            ClassDeclarationSyntax AddExpressionToMethod(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    MethodDeclarationActions methodActions = new MethodDeclarationActions();
                    var addExpressionToMethodAction = methodActions.GetAddExpressionToMethodAction(expression);
                    var newMethodNode = addExpressionToMethodAction(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return AddExpressionToMethod;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddParametersToMethodAction(string methodName, string types, string identifiers)
        {
            ClassDeclarationSyntax AddParametersToMethod(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                // if we have more than one method with same name return without making changes
                var methodNode = GetMethodNode(node, methodName);
                if (methodNode != null)
                {
                    MethodDeclarationActions methodActions = new MethodDeclarationActions();
                    var addParametersToMethodAction = methodActions.GetAddParametersToMethodAction(types, identifiers);
                    var newMethodNode = addParametersToMethodAction(syntaxGenerator, methodNode);
                    node = node.ReplaceNode(methodNode, newMethodNode);
                }
                return node;
            }
            return AddParametersToMethod;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetReplacePublicMethodsBodyAction(string expression)
        {
            ClassDeclarationSyntax ReplaceMethodModifiers(SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node)
            {
                var allMembers = node.Members.ToList();
                var allMethods = allMembers.OfType<MethodDeclarationSyntax>().Where(m => m.Modifiers.Any(mod => mod.Text == Constants.Public));

                while(allMethods != null && allMethods.Count() > 0)
                {
                    var method = allMethods.FirstOrDefault();
                    //ActionResult is a catch all return type
                    MethodDeclarationActions methodActions = new MethodDeclarationActions();
                    var addCommentActionFunc = methodActions.GetAddCommentAction(Constants.MonolithServiceComment);
                    var newMethod = addCommentActionFunc(syntaxGenerator, method);

                    bool asyncCheck = method.Modifiers.Any(mod => mod.Text == Constants.Async);
                    string returnType = asyncCheck ? Constants.TaskActionResult : Constants.ActionResult;
                    var newExpression = expression;
                    if (expression.Contains(Constants.MonolithService + "." + Constants.CreateRequest))
                    {
                        newExpression = expression.Insert((asyncCheck ? expression.IndexOf(Constants.MonolithService) : expression.IndexOf(Constants.CreateRequest)+ Constants.CreateRequest.Length), (asyncCheck ? Constants.Await + " " : Constants.DotResult));
                    }

                    newMethod = newMethod.WithBody(null).WithLeadingTrivia(newMethod.GetLeadingTrivia());
                    newMethod = newMethod.WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(newExpression))).WithLeadingTrivia(newMethod.GetLeadingTrivia());
                    newMethod = newMethod.WithReturnType(SyntaxFactory.ParseTypeName(returnType)).WithLeadingTrivia(newMethod.GetLeadingTrivia());
                    newMethod = newMethod.NormalizeWhitespace();

                    node = node.ReplaceNode(method, newMethod.WithLeadingTrivia(newMethod.GetLeadingTrivia()));

                    allMembers = node.Members.ToList();
                    allMethods = allMembers.OfType<MethodDeclarationSyntax>().Where(m => m.Modifiers.Any(mod => mod.Text == Constants.Public) && !m.GetLeadingTrivia().ToFullString().Contains(Constants.MonolithServiceComment));
                }

                return node;
            }
            return ReplaceMethodModifiers;
        }

        private MethodDeclarationSyntax GetMethodNode(ClassDeclarationSyntax node, string methodName)
        {
            var methodNodeList = node.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(method => method.Identifier.Text == methodName);
            if (methodNodeList != null && methodNodeList.Count() > 1)
            {
                return null;
            }
            return methodNodeList.FirstOrDefault();
        }
    }
}
