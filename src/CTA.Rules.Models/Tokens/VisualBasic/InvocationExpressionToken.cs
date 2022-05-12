using System;
using CTA.Rules.Config;

namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class InvocationExpressionToken : VisualBasicNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (InvocationExpressionToken)obj;
            return token?.Key == this.Key && token?.Namespace == this.Namespace && token?.Type == this.Type;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Namespace, Type);
        }
    }
}
