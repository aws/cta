using CTA.WebForms.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CTA.WebForms.Tests.Extensions
{
    public class CommentingExtensionTests
    {
        private const string TestStatementText = "var x = 10;";
        private const string TestCommentPrependChars = "// ";
        private const string TestStatementCommentShort = "This is a short comment";
        private const string TestClassName = "TestClass1";

        private static string TestCommentText => TestCommentPrependChars + TestStatementCommentShort;
        private static string BraceTestCommentText => $"    {TestCommentText}";

        private ClassDeclarationSyntax _testClassDeclaration;
        private StatementSyntax _singleStatement;
        private IEnumerable<StatementSyntax> _statementCollection;

        [SetUp]
        public void SetUp()
        {
            _testClassDeclaration = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(TestClassName));
            _singleStatement = SyntaxFactory.ParseStatement(TestStatementText);

            _statementCollection = new[]
            {
                SyntaxFactory.ParseStatement(TestStatementText),
                SyntaxFactory.ParseStatement(TestStatementText)
            };
        }

        [Test]
        public void AddComment_Prepends_Text_To_Node_With_Double_Slash()
        {
            var expectedOutput = TestCommentText + Environment.NewLine + TestStatementText;
            var statement = _singleStatement.AddComment(new[] { TestStatementCommentShort });

            Assert.AreEqual(expectedOutput, statement.NormalizeWhitespace().ToFullString());
        }

        [TestCase("Single", 10, new[] { "Single" })]
        [TestCase("Double Double", 6, new[] { "Double", "Double" })]
        [TestCase("Four Four Four Four", 8, new[] { "Four Four", "Four Four" })]
        [TestCase("  This     is a long,   long, long,  long    comment ", 9, new[] { "This is a", "long, long,", "long, long", "comment" })]
        public void AddComment_Splits_Text_Before_Prepending_With_Double_Slash(string comment, int splitSize, IEnumerable<string> commentParts)
        {
            string expectedComment = string.Empty;
            foreach (var commentPart in commentParts)
            {
                expectedComment += TestCommentPrependChars + commentPart + Environment.NewLine;
            }

            var expectedOutput = expectedComment + TestStatementText;
            var statement = SyntaxFactory.ParseStatement(TestStatementText);
            // Set soft character limit to over length of test statement to prevent line breaking
            statement = statement.AddComment(comment, splitSize);

            Assert.AreEqual(expectedOutput, statement.NormalizeWhitespace().ToFullString());
        }

        public void AddComment_Does_Not_Overwrite_Existing_Trivia()
        {
            var statement = SyntaxFactory.ParseStatement(TestStatementText)
                .AddComment(TestStatementCommentShort + "1")
                .AddComment(TestStatementCommentShort + "2");

            var trivia = statement.GetLeadingTrivia();

            Assert.AreEqual(2, trivia.Count());
            Assert.AreEqual(TestCommentText + "1", trivia.First().ToFullString());
            Assert.AreEqual(TestCommentText + "2", trivia.Last().ToFullString());
        }

        [Test]
        public void AddComment_Prepends_Leading_Text_To_First_Node()
        {
            var statements = _statementCollection.AddComment(TestStatementCommentShort);

            var firstStatement = statements.First();
            var lastStatement = statements.Last();

            Assert.AreEqual(TestCommentText, firstStatement.GetLeadingTrivia().Single().ToFullString());
            Assert.IsEmpty(firstStatement.GetTrailingTrivia());
            Assert.IsEmpty(lastStatement.GetLeadingTrivia());
            Assert.IsEmpty(lastStatement.GetTrailingTrivia());
        }

        [Test]
        public void AddComment_Appends_Trailing_Text_After_Last_Node()
        {
            var statements = _statementCollection.AddComment(TestStatementCommentShort, isLeading: false);

            var firstStatement = statements.First();
            var lastStatement = statements.Last();

            Assert.IsEmpty(firstStatement.GetLeadingTrivia());
            Assert.IsEmpty(firstStatement.GetTrailingTrivia());
            Assert.IsEmpty(lastStatement.GetLeadingTrivia());
            Assert.AreEqual(TestCommentText, lastStatement.GetTrailingTrivia().Single().ToFullString());
        }

        [Test]
        public void AddClassBlockComment_Places_Leading_Text_Before_Block_Content()
        {
            var classDec = _testClassDeclaration.AddClassBlockComment(TestStatementCommentShort);

            // Class braces tend to have extra white space trivia attached
            // so we filter those out for the check
            Assert.IsEmpty(classDec.OpenBraceToken.LeadingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)));
            Assert.AreEqual(BraceTestCommentText, classDec.OpenBraceToken.TrailingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)).Single().ToFullString());
            Assert.IsEmpty(classDec.CloseBraceToken.LeadingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)));
            Assert.IsEmpty(classDec.CloseBraceToken.TrailingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)));
        }

        [Test]
        public void AddClassBlockComment_Places_Trailing_Text_After_Block_Content()
        {
            var classDec = _testClassDeclaration.AddClassBlockComment(TestStatementCommentShort, atStart: false);

            // Class braces tend to have extra white space trivia attached
            // so we filter those out for the check
            Assert.IsEmpty(classDec.OpenBraceToken.LeadingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)));
            Assert.IsEmpty(classDec.OpenBraceToken.TrailingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)));
            Assert.AreEqual(BraceTestCommentText, classDec.CloseBraceToken.LeadingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)).Single().ToFullString());
            Assert.IsEmpty(classDec.CloseBraceToken.TrailingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)));
        }
    }
}
