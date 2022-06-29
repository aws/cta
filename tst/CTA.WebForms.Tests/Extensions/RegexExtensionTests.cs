using CTA.WebForms.Extensions;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CTA.WebForms.Tests.Extensions
{
    [TestFixture]
    public class RegexExtensionTests
    {
        private static Regex _testRegex => new Regex(@"(?:Hello)|(?:Good Afternoon)");

        private static string TestReplacement(Match m)
        {
            return $"{m.Value} World!";
        }

        private static async Task<string> TestReplacementAsync(Match m)
        {
            return await Task.FromResult($"{m.Value} World!");
        }

        [TestCase("")]
        [TestCase("Helo")]
        [TestCase("GoodddasdasdsadasdAfter Good After Noon")]
        [TestCase("Afternoon Good")]
        public async Task ReplaceAsync_Does_Nothing_When_No_Matches_Present(string testString)
        {
            var output = await _testRegex.ReplaceAsync(testString, TestReplacementAsync);

            Assert.AreEqual(testString, output);
        }

        [TestCase("Hello")]
        [TestCase("Good Afternoon")]
        [TestCase(" asdasd Hello  ")]
        [TestCase(" asdas Good Afternoon  asd asd")]
        [TestCase("Good Good Good Good Good Afternoon Afternoon Afternoon Afternoon")]
        public async Task ReplaceAsync_Result_Is_Same_As_Replace_When_One_Match_Present(string testString)
        {
            var expectedOutput = _testRegex.Replace(testString, TestReplacement);
            var output = await _testRegex.ReplaceAsync(testString, TestReplacementAsync);

            Assert.AreEqual(expectedOutput, output);
        }

        [TestCase("Good Afternoon Hello")]
        [TestCase("asda    Hello Hello Hello asdasd Hello HelloHello Hello asdasd  ")]
        [TestCase("Afternoon Hello Good asdaGood Afternoonsdasd sadasd Helo Good Hello")]
        public async Task ReplaceAsync_Result_Is_Same_As_Replace_When_Multiple_Matches_Present(string testString)
        {
            var expectedOutput = _testRegex.Replace(testString, TestReplacement);
            var output = await _testRegex.ReplaceAsync(testString, TestReplacementAsync);

            Assert.AreEqual(expectedOutput, output);
        }
    }
}
