
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models
{
    public class MemberAccessAction : GenericAction
    {
        public Func<SyntaxGenerator, SyntaxNode, SyntaxNode> MemberAccessActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (MemberAccessAction)obj;
            return action.Key == this.Key
                && action.Value == this.Value
                && action.MemberAccessActionFunc.Method.Name == this.MemberAccessActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode()
                + 3 * Value.GetHashCode()
                + 5 * (MemberAccessActionFunc != null ? MemberAccessActionFunc.Method.Name.GetHashCode() : 0);
        }
    }
}
