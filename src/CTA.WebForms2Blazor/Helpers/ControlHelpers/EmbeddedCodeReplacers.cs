using System;
using System.Text.RegularExpressions;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public static class EmbeddedCodeReplacers
    {
        //DataBind regex might have issue of taking front parentheses
        public static Regex DataBindRegex = new Regex("<%#\\W*(?<expr>[^%>]*)\\s*%>");
        public static Regex SingleExpRegex = new Regex("<%:\\s*(?<expr>[^%>]*)\\s*%>");
        public static Regex DirectiveRegex = new Regex("<%@\\s*(?<expr>[^%>]*)\\s*%>");
        public static Regex AspExpRegex = new Regex("<%\\$\\s*(?<expr>[^%>]*)\\s*%>");
        
        public static string ReplaceDataBind(Match m)
        {
            var expr = m.Groups["expr"].Value;
            var newValue = "@(" + expr + ")";
            return newValue;
        }
        
        public static string ReplaceSingleExpr(Match m)
        {
            var expr = m.Groups["expr"].Value;
            var newValue = "@" + expr;
            return newValue;
        }
        
        public static string ReplaceDirective(Match m)
        {
            throw new NotImplementedException();
        }
        
        public static string ReplaceAspExpr(Match m)
        {
            throw new NotImplementedException();
        }
    }
}
