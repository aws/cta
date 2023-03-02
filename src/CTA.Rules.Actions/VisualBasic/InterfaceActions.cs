using System;
using System.Linq;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using CTA.Rules.Actions.ActionHelpers;

namespace CTA.Rules.Actions.VisualBasic
{
    /// <summary>
    /// List of actions that can run on Interface Declarations
    /// </summary>
    public class InterfaceActions
    {
        public Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax> GetChangeNameAction(string newName)
        {
            InterfaceBlockSyntax ChangeName(SyntaxGenerator syntaxGenerator, InterfaceBlockSyntax node)
            {
                node = node.WithInterfaceStatement(
                    node.InterfaceStatement.WithIdentifier(SyntaxFactory.Identifier(newName)));
                return node;
            }
            return ChangeName;
        }
        public Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax> GetRemoveAttributeAction(string attributeName)
        {
            InterfaceBlockSyntax RemoveAttribute(SyntaxGenerator syntaxGenerator, InterfaceBlockSyntax node)
            {
                var attributeLists = node.InterfaceStatement.AttributeLists;
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

                var newStatement = node.InterfaceStatement.WithAttributeLists(attributeLists);
                return node.WithInterfaceStatement(newStatement);
            }

            return RemoveAttribute;
        }
        public Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax> GetAddAttributeAction(string attribute)
        {
            InterfaceBlockSyntax AddAttribute(SyntaxGenerator syntaxGenerator, InterfaceBlockSyntax node)
            {
                var attributeLists = node.InterfaceStatement.AttributeLists;
                attributeLists = attributeLists.Add(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName(attribute)))).NormalizeWhitespace());

                var newStatement = node.InterfaceStatement.WithAttributeLists(attributeLists);
                return node.WithInterfaceStatement(newStatement);
            }
            return AddAttribute;
        }
        public Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax> GetAddCommentAction(string comment)
        {
            InterfaceBlockSyntax AddComment(SyntaxGenerator syntaxGenerator, InterfaceBlockSyntax node)
            {
                return (InterfaceBlockSyntax)CommentHelper.AddVBComment(node, comment);
            }
            return AddComment;
        }
        public Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax> GetAddMethodAction(string expression)
        {
            InterfaceBlockSyntax AddMethod(SyntaxGenerator syntaxGenerator, InterfaceBlockSyntax node)
            {
                var allMembers = node.Members;
                var methodStatement = ParseMethodStatement(expression).NormalizeWhitespace();
                allMembers = allMembers.Add(methodStatement);
                node = node.WithMembers(allMembers);
                return node;
            }
            return AddMethod;
        }
        public Func<SyntaxGenerator, InterfaceBlockSyntax, InterfaceBlockSyntax> GetRemoveMethodAction(string methodName)
        {
            //TODO  what if there is operator overloading 
            InterfaceBlockSyntax AddMethod(SyntaxGenerator syntaxGenerator, InterfaceBlockSyntax node)
            {
                var allMethods = node.Members.OfType<MethodStatementSyntax>().ToList();
                var removeMethod =
                    allMethods.FirstOrDefault(m => m.Identifier.ToString() == methodName);
                if (removeMethod != null)
                {
                    node = node.RemoveNode(removeMethod, SyntaxRemoveOptions.KeepNoTrivia);
                }

                return node;
            }
            return AddMethod;
        }

        private MethodStatementSyntax ParseMethodStatement(string expression)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(expression);
            return tree.GetRoot().DescendantNodes().OfType<MethodStatementSyntax>().FirstOrDefault();
        }
    }
}
