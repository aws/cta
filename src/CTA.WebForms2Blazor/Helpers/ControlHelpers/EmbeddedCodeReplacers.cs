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
        /// tags (<%# or <%#: and %>) with content that does not contain the end tag
        /// </summary>
        public static Regex DataBindRegex = new Regex(@"<%#:?\s*(?<expr>[^:](?:(?!%>).)*)\s*%>");
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
        /// Regular expression to identify code block syntax, matches start and end
        /// tags (<%# and %>) with content that does not contain the end tag and has
        /// extra conditions to ensure other embedding syntaxes aren't matched accidentally
        /// </summary>
        public static Regex EmbeddedCodeBlockRegex = new Regex(@"<%(?!(?:--)|[#=:@\$]).\s*(?<expr>(?:(?!%>).)*)\s*%>");

        /// <summary>
        /// Regular expression to identify directive name within directive syntax content,
        /// maps directive name attribute to the first string of consecutive non-space characters
        /// </summary>
        public static Regex DirectiveNameRegex = new Regex(@"(?<directiveName>[\S]+)");

        public static string ReplaceOneWayDataBinds(string htmlString)
        {
            return DataBindRegex.Replace(htmlString, match => string.Format(
                Constants.RazorExplicitEmbeddingTemplate,
                match.Groups[EmbeddedExpressionRegexGroupName].Value.Trim()));
        }
        
        public static string ReplaceRawExprs(string htmlString)
        {
            return RawExprRegex.Replace(htmlString, match => string.Format(
                Constants.RazorExplicitRawEmbeddingTemplate,
                match.Groups[EmbeddedExpressionRegexGroupName].Value.Trim()));
        }

        public static string ReplaceHTMLEncodedExprs(string htmlString)
        {
            return HTMLEncodedExprRegex.Replace(htmlString, match => string.Format(
                Constants.RazorExplicitEmbeddingTemplate,
                match.Groups[EmbeddedExpressionRegexGroupName].Value.Trim()));
        }
        
        public static string ReplaceDirectives(string htmlString, string originalFilePath, string projectName, ViewImportService viewImportService)
        {
            return DirectiveRegex.Replace(htmlString, match => ConstructBlazorDirectives(
                match.Groups[EmbeddedExpressionRegexGroupName].Value,
                originalFilePath,
                projectName,
                viewImportService));
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
            return AspCommentRegex.Replace(htmlString, match => string.Format(
                Constants.RazorServerSideCommentTemplate,
                match.Groups[EmbeddedExpressionRegexGroupName].Value.Trim()));
        }

        public static string ReplaceEmbeddedCodeBlocks(string htmlString)
        {
            // TODO: Insert @for, @if, etc. if embedded code contains a conditional
            // or loop statement. This is difficult to do because separation of the
            // statements from each other and the normal statements in the block may
            // cause issues especially considering that multiple embedded blocks can
            // contain different parts of the same loop or condition

            return EmbeddedCodeBlockRegex.Replace(htmlString, match => string.Format(
                Constants.RazorCodeBlockEmbeddingTemplate,
                match.Groups[EmbeddedExpressionRegexGroupName].Value.Trim()));
        }

        public static string ConstructBlazorDirectives(string content, string originalFilePath, string projectName, ViewImportService viewImportService)
        {
            var directiveName = DirectiveNameRegex.Match(content).Groups[DirectiveNameRegexGroupName].Value;
            var directiveConverter = SupportedControls.DirectiveRulesMap.ContainsKey(directiveName) ?
                SupportedControls.DirectiveRulesMap[directiveName] : SupportedControls.DefaultDirectiveConverter;
        
            return directiveConverter.ConvertDirective(directiveName, content.Trim(), originalFilePath, projectName, viewImportService);
        }
    }
}
