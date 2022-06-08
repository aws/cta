using System;

namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class MethodBlockToken : VisualBasicNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (MethodBlockToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullKey);
        }
    }
}
