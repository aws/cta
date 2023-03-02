using System;
using CTA.Rules.Actions.ActionHelpers;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Actions.Csharp
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
                return (AttributeListSyntax)CommentHelper.AddCSharpComment(node, comment);
            }
            return AddComment;
        }
    }
}
