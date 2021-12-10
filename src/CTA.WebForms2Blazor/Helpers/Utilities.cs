using System;

namespace CTA.WebForms2Blazor.Helpers
{
    public class Utilities
    {
        public static string SeparateStringsWithNewLine(params string[] strings)
        {
            return strings == null
                ? string.Empty
                : string.Join(Environment.NewLine, strings);
        }
    }
}
