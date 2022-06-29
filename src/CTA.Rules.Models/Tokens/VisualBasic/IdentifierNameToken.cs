
using System;
using CTA.Rules.Models.Tokens.VisualBasic;

namespace CTA.Rules.Models.VisualBasic
{
    public class IdentifierNameToken : VisualBasicNodeToken
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
