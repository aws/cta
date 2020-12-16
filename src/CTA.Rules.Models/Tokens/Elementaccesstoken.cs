
using CTA.Rules.Models.Tokens;

namespace CTA.Rules.Models
{
    public class ElementAccessToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (ElementAccessToken)obj;
            return token.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return 7 * FullKey.GetHashCode();
        }
    }
}