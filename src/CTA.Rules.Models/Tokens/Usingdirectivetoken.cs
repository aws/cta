namespace CTA.Rules.Models.Tokens
{

    public class UsingDirectiveToken : NodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (UsingDirectiveToken)obj;
            return token.Key == this.Key;
        }
        public override int GetHashCode()
        {
            return 23 * Key.GetHashCode();
        }
    }
}