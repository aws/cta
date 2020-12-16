using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.Rules.Models.Tokens
{
    public class MethodDeclarationToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (MethodDeclarationToken)obj;
            return token.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return 29 * FullKey.GetHashCode();
        }
    }
}
