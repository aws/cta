using System;


namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class VBMemberAccessToken : VisualBasicNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (VBMemberAccessToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullKey);
        }
    }
}
