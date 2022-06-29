﻿
using System;
using CTA.Rules.Models.Tokens;

namespace CTA.Rules.Models
{

    public class MemberAccessToken : CsharpNodeToken
    {
        public override bool Equals(object obj)
        {
            var token = (MemberAccessToken)obj;
            return token?.FullKey == this.FullKey;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FullKey);
        }
    }
}
