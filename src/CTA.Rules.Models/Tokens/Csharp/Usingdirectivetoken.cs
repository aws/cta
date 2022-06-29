using System;

namespace CTA.Rules.Models.Tokens
{

    public class UsingDirectiveToken : CsharpNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (UsingDirectiveToken)obj;
            return token?.Key == this.Key;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key);
        }
    }
}
