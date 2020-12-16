
using CTA.Rules.Models.Tokens;

namespace CTA.Rules.Models
{
    public class InterfaceDeclarationToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (InterfaceDeclarationToken)obj;
            return token.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return 5 * FullKey.GetHashCode();
        }
    }
}