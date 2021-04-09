using Microsoft.CodeAnalysis;

namespace CTA.Rules.Models
{
    public class TextChange
    {
        public string NewText { get; set; }
        public FileLinePositionSpan FileLinePositionSpan { get; set; }
    }
}
