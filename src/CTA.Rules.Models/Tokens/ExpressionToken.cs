using System;
using CTA.Rules.Config;

namespace CTA.Rules.Models.Tokens
{
    public class ExpressionToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (ExpressionToken)obj;
            return token?.Key == this.Key && token?.Namespace == this.Namespace && token?.Type == this.Type;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Namespace, Type);
        }
    }
}
