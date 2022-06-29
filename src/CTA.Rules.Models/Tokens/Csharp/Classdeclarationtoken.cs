﻿
using System;
using CTA.Rules.Models.Tokens;

namespace CTA.Rules.Models
{
    public class ClassDeclarationToken : CsharpNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (ClassDeclarationToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullKey);
        }
    }
}
