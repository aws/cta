using System;

namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class AccessorBlockToken : VisualBasicNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (AccessorBlockToken)obj;
            return token?.Type == this.Type && token?.Namespace == this.Namespace && token?.Key.Trim() == this.Key.Trim();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Namespace, Type);
        }
    }
}
