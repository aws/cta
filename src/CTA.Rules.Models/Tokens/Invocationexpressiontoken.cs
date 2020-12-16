using CTA.Rules.Config;

namespace CTA.Rules.Models.Tokens
{
    public class InvocationExpressionToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (InvocationExpressionToken)obj;
            return token.Key == this.Key && token.Namespace == this.Namespace && token.Type == this.Type;
        }
        public override int GetHashCode()
        {
            return 3 * Key.GetHashCode() + Utils.GenerateHashCode(11, this.Namespace) + Utils.GenerateHashCode(13, Type);
        }
    }
}