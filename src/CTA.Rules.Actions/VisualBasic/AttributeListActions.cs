using System;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.VisualBasic
{
    /// <summary>
    /// List of actions that can run on attribute lists
    /// </summary>
    public class AttributeListActions
    {
        public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> GetAddCommentAction(string comment)
        {
            AttributeListSyntax AddComment(SyntaxGenerator syntaxGenerator, AttributeListSyntax node)
            {
                //TODO IS there possibility of NPE , if there are no Trivia or it always returns a node...
                var currentTrivia = node.GetLeadingTrivia();
                currentTrivia = currentTrivia.Insert(0,
                    SyntaxFactory.SyntaxTrivia(SyntaxKind.CommentTrivia,
                        string.Format(Constants.VbCommentFormat, comment)));
                node = node.WithLeadingTrivia(currentTrivia).NormalizeWhitespace();
                return node;
            }
            return AddComment;
        }
    }
}
