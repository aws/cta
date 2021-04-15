using System;
using Microsoft.CodeAnalysis;

namespace CTA.Rules.Models
{
    public class TextChange
    {
        public string NewText { get; set; }
        public FileLinePositionSpan FileLinePositionSpan { get; set; }

        public bool Equals(TextChange textChange)
        {
            if (textChange == null) return false;

            return NewText == textChange.NewText
                && FileLinePositionSpan.Equals(textChange.FileLinePositionSpan);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TextChange);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileLinePositionSpan, NewText);
        }
    }
}
