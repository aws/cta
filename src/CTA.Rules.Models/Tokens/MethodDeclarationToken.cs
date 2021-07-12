using System;

namespace CTA.Rules.Models.Tokens
{
    public class MethodDeclarationToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (MethodDeclarationToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(29 * FullKey?.GetHashCode() ?? 0);
        }
    }
}
