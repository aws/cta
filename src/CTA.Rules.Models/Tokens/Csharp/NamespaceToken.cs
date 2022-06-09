using System;

namespace CTA.Rules.Models.Tokens
{

    public class NamespaceToken : CsharpNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (NamespaceToken)obj;
            return token?.Key == this.Key;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key);
        }
    }
}
