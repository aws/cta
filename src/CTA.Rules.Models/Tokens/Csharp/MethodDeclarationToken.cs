using System;

namespace CTA.Rules.Models.Tokens
{
    public class MethodDeclarationToken : CsharpNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (MethodDeclarationToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullKey);
        }
    }
}
