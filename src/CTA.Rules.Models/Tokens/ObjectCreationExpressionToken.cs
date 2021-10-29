using System;
using CTA.Rules.Config;

namespace CTA.Rules.Models.Tokens
{

    public class ObjectCreationExpressionToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (ObjectCreationExpressionToken)obj;
            return token?.Key == this.Key;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Namespace, Type);
        }
    }
}
