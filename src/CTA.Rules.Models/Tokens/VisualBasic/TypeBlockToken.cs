
using System;
using CTA.Rules.Models.Tokens.VisualBasic;

namespace CTA.Rules.Models.VisualBasic
{
    public class TypeBlockToken : VisualBasicNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (ClassDeclarationToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullKey);
        }
    }
}
