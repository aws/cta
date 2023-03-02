using System;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using CTA.Rules.Actions.ActionHelpers;

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
                return (AttributeListSyntax)CommentHelper.AddVBComment(node, comment);
            }
            return AddComment;
        }
    }
}
