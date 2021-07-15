using System;
using System.Text.RegularExpressions;
using CTA.WebForms2Blazor.Services;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public static class EmbeddedCodeReplacers
    {
        public const string BoundAttrNameGroupName = "boundAttrName";
        public const string EmbeddedExpressionRegexGroupName = "expr";
        public const string AspExpressionTypeGroupName = "exprType";
        public const string DirectiveNameRegexGroupName = "directiveName";

        /// <summary>
        /// Regular expression to identify data binding syntax, matches start and end
        /// tags (<%# and %>) with content that does not contain the end tag
        /// </summary>
        public static Regex DataBindRegex = new Regex(@"<%#\s*(?<expr>(?:(?!%>).)*)\s*%>");
        /// <summary>
        /// Regular expression to identify raw expression syntax, matches start and end
        /// tags (<%= and %>) with content that does not contain the end tag
        /// </summary>
        public static Regex RawExprRegex = new Regex(@"<%=\s*(?<expr>(?:(?!%>).)*)\s*%>");
        /// <summary>
        /// Regular expression to identify html encoded expression syntax, matches start
        /// and end tags (<%: and %>) with content that does not contain the end tag
        /// </summary>
        public static Regex HTMLEncodedExprRegex = new Regex(@"<%:\s*(?<expr>(?:(?!%>).)*)\s*%>");
        /// <summary>
        /// Regular expression to identify directive syntax, matches start and end
        /// tags (<%@ and %>) with content that does not contain the end tag
        /// </summary>
        public static Regex DirectiveRegex = new Regex(@"<%@\s*(?<expr>(?:(?!%>).)*)\s*%>");
        /// <summary>
        /// Regular expression to identify asp expression syntax, matches start and end
        /// tags (<%$ and %>) with content that does not contain the end tag
        /// </summary>
        public static Regex AspExpRegex = new Regex(@"<%\$\s*(?<exprType>[^*]+):\s*(?<expr>(?:(?!%>).)+)\s*%>");
        /// <summary>
        /// Regular expression to identify asp comment syntax, matches start and end tags
        /// (<%-- and --%>) with content that does not contain the end tag
        /// </summary>
        public static Regex AspCommentRegex = new Regex(@"<%--\s*(?<expr>(?:(?!--%>).)*)\s*--%>");

        /// <summary>
        /// Regular expression to identify directive name within directive syntax content,
        /// maps directive name attribute to the first string of consecutive non-space characters
        /// </summary>
        public static Regex DirectiveNameRegex = new Regex(@"(?<directiveName>[\S]+)");

        public static string ReplaceOneWayDataBinds(string htmlString)
        {
            return DataBindRegex.Replace(htmlString, match => ConstructOneWayBinding(match.Groups[EmbeddedExpressionRegexGroupName].Value));
        }
        
        public static string ReplaceRawExprs(string htmlString)
        {
            return RawExprRegex.Replace(htmlString, match => ConstructRawOneWayBinding(match.Groups[EmbeddedExpressionRegexGroupName].Value));
        }

        public static string ReplaceHTMLEncodedExprs(string htmlString)
        {
            return HTMLEncodedExprRegex.Replace(htmlString, match => ConstructOneWayBinding(match.Groups[EmbeddedExpressionRegexGroupName].Value));
        }
        
        public static string ReplaceDirectives(string htmlString, ViewImportService viewImportService)
        {
            return DirectiveRegex.Replace(htmlString, match => ConstructBlazorDirectives(match.Groups[EmbeddedExpressionRegexGroupName].Value, viewImportService));
        }
        
        public static string ReplaceAspExprs(string htmlString)
        {
            return AspExpRegex.Replace(htmlString, match =>
            {
                var exprType = match.Groups[AspExpressionTypeGroupName].Value;
                var expr = match.Groups[EmbeddedExpressionRegexGroupName].Value;

                return SupportedControls.AspExpressionRulesMap.ContainsKey(exprType) ?
                    SupportedControls.AspExpressionRulesMap[exprType](expr.Trim()) :
                    SupportedControls.DefaultAspExpressionConverter(exprType, expr.Trim());
            });
        }

        public static string ReplaceAspComments(string htmlString)
        {
            return AspCommentRegex.Replace(htmlString, match => ConstructRazorServerSideComment(match.Groups[EmbeddedExpressionRegexGroupName].Value));
        }

        public static string ConstructRawOneWayBinding(string content)
        {
            return string.Format(Constants.RazorExplicitRawEmbeddingTemplate, content.Trim());
        }

        public static string ConstructOneWayBinding(string content)
        {
            return string.Format(Constants.RazorExplicitEmbeddingTemplate, content.Trim());
        }

        public static string ConstructRazorServerSideComment(string content)
        {
            return string.Format(Constants.RazorServerSideCommentTemplate, content.Trim());
        }

        public static string ConstructBlazorDirectives(string content, ViewImportService viewImportService)
        {
            var directiveName = DirectiveNameRegex.Match(content).Groups[DirectiveNameRegexGroupName].Value;
            var directiveConverter = SupportedControls.DirectiveRulesMap.ContainsKey(directiveName) ?
                SupportedControls.DirectiveRulesMap[directiveName] : SupportedControls.DefaultDirectiveConverter;

            return directiveConverter.ConvertDirective(directiveName, content.Trim(), viewImportService);
        }
    }
}
