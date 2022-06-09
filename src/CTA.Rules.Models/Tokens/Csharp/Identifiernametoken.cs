
using System;
using CTA.Rules.Config;
using CTA.Rules.Models.Tokens;

namespace CTA.Rules.Models
{
    public class IdentifierNameToken : CsharpNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (IdentifierNameToken)obj;
            return token?.Key == this.Key && token?.Namespace == this.Namespace;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Namespace);
        }
    }
}
