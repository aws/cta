using System;


namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class MemberAccessToken : VisualBasicNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (MemberAccessToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullKey);
        }
    }
}
