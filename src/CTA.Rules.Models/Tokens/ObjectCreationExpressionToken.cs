using CTA.Rules.Config;

namespace CTA.Rules.Models.Tokens
{

    public class ObjectCreationExpressionToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (ObjectCreationExpressionToken)obj;
            return token.Key == this.Key;
        }
        public override int GetHashCode()
        {
            return 3 * this.Key.GetHashCode() + Utils.GenerateHashCode(5, this.Namespace) + Utils.GenerateHashCode(7, this.Type);
        }
    }
}