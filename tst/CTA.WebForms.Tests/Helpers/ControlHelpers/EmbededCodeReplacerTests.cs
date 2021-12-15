using CTA.WebForms.Helpers.ControlHelpers;
using NUnit.Framework;

namespace CTA.WebForms.Tests.Helpers.ControlHelpers
{
    public class EmbededCodeReplacerTests
    {
        private const string NormalExpressionContent = "normal expression content";
        private const string MutualTagCharExpressionContent = "mutual %_tag_% chars";

        private const string AppSettingsSectionName = "appsettings";
        private const string AppSettingsAspExpressionType = "AppSettings";
        private const string UnknownAspExpressionType = "RouteBinding";
        private const string AppSettingsAspExpressionContent = "setting1";
        private const string UnknownAspExpressionContent = "routeData[\"location\"]";

        private const string WebFormsOneWayDataBindStartTag = "<%#:";
        private const string WebFormsRawExpressionStartTag = "<%=";
        private const string WebFormsHTMLEncodedExpressionStartTag = "<%:";
        private const string WebFormsDirectiveStartTag = "<%@";
        private const string WebFormsAspExpressionStartTag = "<%$";
        private const string WebFormsCommentStartTag = "<%--";
        private const string WebFormsCodeBlockStartTag = "<%";
        private const string WebFormsNormalEndTag = "%>";
        private const string WebFormsCommentEndTag = "--%>";
        private const string WebFormsMalformedEndTag = "/>";
        private const string WebFormsMalformedCommentEndTag = "-->";

        private const string RazorExplicitBindingStartTag = "@(";
        private const string RazorExplicitBindingEndTag = ")";
        private const string RazorCommentStartTag = "@*";
        private const string RazorCommentEndTag = "*@";
        private const string RazorCodeBlockStartTag = "@{";
        private const string RazorCodeBlockEndTag = "}";

        [Test]
        public void DataBindRegex_Matches_Normal_Correct_Expressions()
        {
            var inputText = $"{WebFormsOneWayDataBindStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.DataBindRegex.IsMatch(inputText));
        }

        [Test]
        public void DataBindRegex_Matches_Expressions_With_Characters_Mutual_To_Tag_Braces()
        {
            var inputText = $"{WebFormsOneWayDataBindStartTag} {MutualTagCharExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.DataBindRegex.IsMatch(inputText));
        }

        [Test]
        public void DataBindRegex_Does_Not_Match_Expressions_With_Malformed_Tag_Braces()
        {
            var inputText = $"{WebFormsOneWayDataBindStartTag} {NormalExpressionContent} {WebFormsMalformedEndTag}";

            Assert.False(EmbeddedCodeReplacers.DataBindRegex.IsMatch(inputText));
        }

        [Test]
        public void RawExprRegex_Matches_Normal_Correct_Expressions()
        {
            var inputText = $"{WebFormsRawExpressionStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.RawExprRegex.IsMatch(inputText));
        }

        [Test]
        public void RawExprRegex_Matches_Expressions_With_Characters_Mutual_To_Tag_Braces()
        {
            var inputText = $"{WebFormsRawExpressionStartTag} {MutualTagCharExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.RawExprRegex.IsMatch(inputText));
        }

        [Test]
        public void RawExprRegex_Does_Not_Match_Expressions_With_Malformed_Tag_Braces()
        {
            var inputText = $"{WebFormsRawExpressionStartTag} {NormalExpressionContent} {WebFormsMalformedEndTag}";

            Assert.False(EmbeddedCodeReplacers.RawExprRegex.IsMatch(inputText));
        }

        [Test]
        public void HTMLEncodedExprRegex_Matches_Normal_Correct_Expressions()
        {
            var inputText = $"{WebFormsHTMLEncodedExpressionStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.HTMLEncodedExprRegex.IsMatch(inputText));
        }

        [Test]
        public void HTMLEncodedExprRegex_Matches_Expressions_With_Characters_Mutual_To_Tag_Braces()
        {
            var inputText = $"{WebFormsHTMLEncodedExpressionStartTag} {MutualTagCharExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.HTMLEncodedExprRegex.IsMatch(inputText));
        }

        [Test]
        public void HTMLEncodedExprRegex_Does_Not_Match_Expressions_With_Malformed_Tag_Braces()
        {
            var inputText = $"{WebFormsHTMLEncodedExpressionStartTag} {NormalExpressionContent} {WebFormsMalformedEndTag}";

            Assert.False(EmbeddedCodeReplacers.HTMLEncodedExprRegex.IsMatch(inputText));
        }

        [Test]
        public void DirectiveRegex_Matches_Normal_Correct_Expressions()
        {
            var inputText = $"{WebFormsDirectiveStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.DirectiveRegex.IsMatch(inputText));
        }

        [Test]
        public void DirectiveRegex_Matches_Expressions_With_Characters_Mutual_To_Tag_Braces()
        {
            var inputText = $"{WebFormsDirectiveStartTag} {MutualTagCharExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.DirectiveRegex.IsMatch(inputText));
        }

        [Test]
        public void DirectiveRegex_Does_Not_Match_Expressions_With_Malformed_Tag_Braces()
        {
            var inputText = $"{WebFormsDirectiveStartTag} {NormalExpressionContent} {WebFormsMalformedEndTag}";

            Assert.False(EmbeddedCodeReplacers.DirectiveRegex.IsMatch(inputText));
        }

        [Test]
        public void AspExpRegex_Matches_Normal_Correct_Expressions()
        {
            var inputText = $"{WebFormsAspExpressionStartTag} {AppSettingsAspExpressionType}: {AppSettingsAspExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.AspExpRegex.IsMatch(inputText));
        }

        [Test]
        public void AspExpRegex_Does_Not_Match_Expressions_With_Malformed_Tag_Braces()
        {
            var inputText = $"{WebFormsAspExpressionStartTag} {AppSettingsAspExpressionType}: {AppSettingsAspExpressionContent} {WebFormsMalformedEndTag}";

            Assert.False(EmbeddedCodeReplacers.AspExpRegex.IsMatch(inputText));
        }

        [Test]
        public void AspCommentRegex_Matches_Normal_Correct_Expressions()
        {
            var inputText = $"{WebFormsCommentStartTag} {NormalExpressionContent} {WebFormsCommentEndTag}";

            Assert.True(EmbeddedCodeReplacers.AspCommentRegex.IsMatch(inputText));
        }

        [Test]
        public void AspCommentRegex_Matches_Expressions_With_Characters_Mutual_To_Tag_Braces()
        {
            var inputText = $"{WebFormsCommentStartTag} {MutualTagCharExpressionContent} {WebFormsCommentEndTag}";

            Assert.True(EmbeddedCodeReplacers.AspCommentRegex.IsMatch(inputText));
        }

        [Test]
        public void AspCommentRegex_Does_Not_Match_Expressions_With_Malformed_Tag_Braces()
        {
            var inputText = $"{WebFormsCommentStartTag} {NormalExpressionContent} {WebFormsMalformedCommentEndTag}";

            Assert.False(EmbeddedCodeReplacers.AspCommentRegex.IsMatch(inputText));
        }

        [Test]
        public void EmbeddedCodeBlockRegex_Matches_Normal_Correct_Expressions()
        {
            var inputText = $"{WebFormsCodeBlockStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.EmbeddedCodeBlockRegex.IsMatch(inputText));
        }

        [Test]
        public void EmbeddedCodeBlockRegex_Matches_Expressions_With_Characters_Mutual_To_Tag_Braces()
        {
            var inputText = $"{WebFormsCodeBlockStartTag} {MutualTagCharExpressionContent} {WebFormsNormalEndTag}";

            Assert.True(EmbeddedCodeReplacers.EmbeddedCodeBlockRegex.IsMatch(inputText));
        }

        [Test]
        public void EmbeddedCodeBlockRegex_Does_Not_Match_Other_Embedding_Syntaxes()
        {
            var directiveSyntax = $"{WebFormsDirectiveStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";
            var commentSyntax = $"{WebFormsCommentStartTag} {NormalExpressionContent} {WebFormsCommentEndTag}";
            var aspExprSyntax = $"{WebFormsAspExpressionStartTag} {AppSettingsAspExpressionType}: {AppSettingsAspExpressionContent} {WebFormsNormalEndTag}";

            Assert.False(EmbeddedCodeReplacers.EmbeddedCodeBlockRegex.IsMatch(directiveSyntax));
            Assert.False(EmbeddedCodeReplacers.EmbeddedCodeBlockRegex.IsMatch(commentSyntax));
            Assert.False(EmbeddedCodeReplacers.EmbeddedCodeBlockRegex.IsMatch(aspExprSyntax));
        }

        [Test]
        public void ReplaceOneWayDataBinds_Properly_Reformats_Tag()
        {
            var inputText = $"{WebFormsOneWayDataBindStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";
            var expectedOutput = $"{RazorExplicitBindingStartTag}{NormalExpressionContent}{RazorExplicitBindingEndTag}";
            var actualOutput = EmbeddedCodeReplacers.ReplaceOneWayDataBinds(inputText);

            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void ReplaceRawExprs_Properly_Reformats_Tag()
        {
            var inputText = $"{WebFormsRawExpressionStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";
            var expectedOutput = $"{RazorExplicitBindingStartTag}new MarkupString({NormalExpressionContent}){RazorExplicitBindingEndTag}";
            var actualOutput = EmbeddedCodeReplacers.ReplaceRawExprs(inputText);

            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void ReplaceHTMLEncodedExprs_Properly_Reformats_Tag()
        {
            var inputText = $"{WebFormsHTMLEncodedExpressionStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";
            var expectedOutput = $"{RazorExplicitBindingStartTag}{NormalExpressionContent}{RazorExplicitBindingEndTag}";
            var actualOutput = EmbeddedCodeReplacers.ReplaceHTMLEncodedExprs(inputText);

            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void ReplaceAspExprs_Properly_Replaces_AppSettings_Type_Tag()
        {
            var inputText = $"{WebFormsAspExpressionStartTag} {AppSettingsAspExpressionType}: {AppSettingsAspExpressionContent} {WebFormsNormalEndTag}";
            var expectedOutput = $"{RazorExplicitBindingStartTag}Configuration[\"{AppSettingsSectionName}:{AppSettingsAspExpressionContent}\"]{RazorExplicitBindingEndTag}";
            var actualOutput = EmbeddedCodeReplacers.ReplaceAspExprs(inputText);

            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void ReplaceAspExprs_Properly_Replaces_Unknown_Type_Tag()
        {
            var inputText = $"{WebFormsAspExpressionStartTag} {UnknownAspExpressionType}: {UnknownAspExpressionContent} {WebFormsNormalEndTag}";
            var expectedOutput = $"@({UnknownAspExpressionType}.{UnknownAspExpressionContent})";
            var actualOutput = EmbeddedCodeReplacers.ReplaceAspExprs(inputText);

            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void ReplaceAspComments_Properly_Reformats_Server_Side_Comments()
        {
            var inputText = $"{WebFormsCommentStartTag} {NormalExpressionContent} {WebFormsCommentEndTag}";
            var expectedOutput = $"{RazorCommentStartTag} {NormalExpressionContent} {RazorCommentEndTag}";
            var actualOutput = EmbeddedCodeReplacers.ReplaceAspComments(inputText);

            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void ReplaceEmbeddedCodeBlocks_Properly_Reformats_Code_Blocks()
        {
            var inputText = $"{WebFormsCodeBlockStartTag} {NormalExpressionContent} {WebFormsNormalEndTag}";
            var expectedOutput = $"{RazorCodeBlockStartTag} {NormalExpressionContent} {RazorCodeBlockEndTag}";
            var actualOutput = EmbeddedCodeReplacers.ReplaceEmbeddedCodeBlocks(inputText);

            Assert.AreEqual(expectedOutput, actualOutput);
        }
    }
}
