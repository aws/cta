using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Linq;

namespace CTA.Rules.Actions
{
    /// <summary>
    /// List of actions that can run on Class Declarations
    /// </summary>
    public class ClassActions
    {
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRemoveBaseClassAction(string baseClass)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> RemoveBaseClass = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
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

                if (newBaseTypes.Count == 0)
                {
                    node = node.WithBaseList(null);
                }
                else
                {
                    node = node.WithBaseList(node.BaseList.WithTypes(newBaseTypes));
                }
                return node;
            };

            return RemoveBaseClass;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddBaseClassAction(string baseClass)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> AddBaseClass = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
            {
                node = (ClassDeclarationSyntax)syntaxGenerator.AddBaseType(node, SyntaxFactory.ParseName(baseClass));
                return node;
            };
            return AddBaseClass;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetChangeNameAction(string className)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> ChangeName = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
            {
                node = node.WithIdentifier(SyntaxFactory.Identifier(className)).NormalizeWhitespace();
                return node;
            };
            return ChangeName;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRemoveAttributeAction(string attributeName)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> RemoveAttribute = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
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
            };

            return RemoveAttribute;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddAttributeAction(string attribute)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> AddAttribute = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
            {
                var attributeLists = node.AttributeLists;
                attributeLists = attributeLists.Add(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName(attribute)))));

                node = node.WithAttributeLists(attributeLists).NormalizeWhitespace();
                return node;
            };
            return AddAttribute;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddCommentAction(string comment)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> AddComment = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                //TODO see if this will lead NPE    
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            };
            return AddComment;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetAddMethodAction(string expression)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> AddMethod = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
            {
                var allMembers = node.Members;
                allMembers = allMembers.Add(SyntaxFactory.ParseMemberDeclaration(expression));
                node = node.WithMembers(allMembers).NormalizeWhitespace();
                return node;
            };
            return AddMethod;
        }

        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRemoveMethodAction(string methodName)
        {
            //TODO  what if there is operator overloading 
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> AddMethod = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
            {
                var allMembers = node.Members.ToList();
                var allMethods = allMembers.OfType<MethodDeclarationSyntax>();
                if(allMethods.Count() > 0)
                {
                    var removeMethod = allMethods.Where(m => m.Identifier.ToString() == methodName);
                    if(removeMethod.Count() > 0)
                    {
                        node = node.RemoveNode(removeMethod.FirstOrDefault(), SyntaxRemoveOptions.KeepNoTrivia).NormalizeWhitespace();
                    }
                }
                
                return node;
            };
            return AddMethod;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetRenameClassAction(string newClassName)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> RenameClass = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
            {                
                node = node.WithIdentifier(SyntaxFactory.Identifier(newClassName)).NormalizeWhitespace();
                return node;
            };
            return RenameClass;
        }
        public Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> GetReplaceMethodModifiersAction(string methodName, string modifiers)
        {
            Func<SyntaxGenerator, ClassDeclarationSyntax, ClassDeclarationSyntax> ReplaceMethodModifiers = (SyntaxGenerator syntaxGenerator, ClassDeclarationSyntax node) =>
            {
                var allMembers = node.Members.ToList();
                var allMethods = allMembers.OfType<MethodDeclarationSyntax>();
                if(allMethods.Count() > 0)
                {
                    var replaceMethod = allMethods.Where(m => m.Identifier.ToString() == methodName);
                    if(replaceMethod.Count() > 0)
                    {
                        SyntaxTokenList tokenList = new SyntaxTokenList(SyntaxFactory.ParseTokens(modifiers));
                        var newMethod = replaceMethod.FirstOrDefault().WithModifiers(tokenList);

                        node = node.WithMembers(node.Members.Replace(replaceMethod.FirstOrDefault(), newMethod)).NormalizeWhitespace();
                    }
                }

                return node;
            };
            return ReplaceMethodModifiers;
        }
    }
}
