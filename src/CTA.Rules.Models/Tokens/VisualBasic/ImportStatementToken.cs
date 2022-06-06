using System;

namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class ImportStatementToken : VisualBasicNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (ImportStatementToken)obj;
            return token?.Key == this.Key;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Key);
        }
    }
}
