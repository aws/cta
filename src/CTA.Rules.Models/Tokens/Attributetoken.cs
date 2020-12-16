
using CTA.Rules.Config;
using CTA.Rules.Models.Tokens;

namespace CTA.Rules.Models
{
    public class AttributeToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (AttributeToken)obj;
            return token.Type == this.Type && token.Namespace == this.Namespace && token.Key.Trim() == this.Key.Trim();
        }

        public override int GetHashCode()
        {
            return 3 * this.Key.GetHashCode() + Utils.GenerateHashCode(5, this.Namespace) + Utils.GenerateHashCode(7, this.Type);
        }
    }
}