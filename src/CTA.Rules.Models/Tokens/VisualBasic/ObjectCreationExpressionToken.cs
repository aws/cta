using System;

namespace CTA.Rules.Models.Tokens.VisualBasic;

public class ObjectCreationExpressionToken : VisualBasicNodeToken
{
    public override bool Equals(object obj)
    {
        var token = (ObjectCreationExpressionToken)obj;
        return token?.Key == this.Key;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Namespace, Type);
    }
}
