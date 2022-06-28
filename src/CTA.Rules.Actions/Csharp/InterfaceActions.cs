using System;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.Csharp
{
    /// <summary>
    /// List of actions that can run on Interface Declarations
    /// </summary>
    public class InterfaceActions
    {
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetChangeNameAction(string newName)
        {
            InterfaceDeclarationSyntax ChangeName(SyntaxGenerator syntaxGenerator, InterfaceDeclarationSyntax node)
            {
                node = node.WithIdentifier(SyntaxFactory.Identifier(newName));
                return node;
            }
            return ChangeName;
        }
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetRemoveAttributeAction(string attributeName)
        {
            InterfaceDeclarationSyntax RemoveAttribute(SyntaxGenerator syntaxGenerator, InterfaceDeclarationSyntax node)
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

                node = node.WithAttributeLists(attributeLists);
                return node;
            }

            return RemoveAttribute;
        }
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetAddAttributeAction(string attribute)
        {
            InterfaceDeclarationSyntax AddAttribute(SyntaxGenerator syntaxGenerator, InterfaceDeclarationSyntax node)
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
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetAddCommentAction(string comment)
        {
            InterfaceDeclarationSyntax AddComment(SyntaxGenerator syntaxGenerator, InterfaceDeclarationSyntax node)
            {
                SyntaxTriviaList currentTrivia = node.GetLeadingTrivia();
                //TODO see if this will lead NPE    
                currentTrivia = currentTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, string.Format(Constants.CommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetAddMethodAction(string expression)
        {
            InterfaceDeclarationSyntax AddMethod(SyntaxGenerator syntaxGenerator, InterfaceDeclarationSyntax node)
            {
                var allMembers = node.Members;
                allMembers = allMembers.Add(SyntaxFactory.ParseMemberDeclaration(expression));
                node = node.WithMembers(allMembers).NormalizeWhitespace();
                return node;
            }
            return AddMethod;
        }
        public Func<SyntaxGenerator, InterfaceDeclarationSyntax, InterfaceDeclarationSyntax> GetRemoveMethodAction(string methodName)
        {
            //TODO  what if there is operator overloading 
            InterfaceDeclarationSyntax AddMethod(SyntaxGenerator syntaxGenerator, InterfaceDeclarationSyntax node)
            {
                var allMembers = node.Members.ToList();
                var allMethods = allMembers.OfType<MethodDeclarationSyntax>();
                var removeMethod = allMethods.Where(m => m.Identifier.ToString() == methodName).FirstOrDefault();
                allMembers.Remove(removeMethod);
                node = node.RemoveNode(removeMethod, SyntaxRemoveOptions.KeepNoTrivia);
                return node;
            }
            return AddMethod;
        }
    }
}
