﻿using System;
using CTA.Rules.Models.Tokens;

namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class InterfaceBlockToken : VisualBasicNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (InterfaceBlockToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullKey);
        }
    }
}
