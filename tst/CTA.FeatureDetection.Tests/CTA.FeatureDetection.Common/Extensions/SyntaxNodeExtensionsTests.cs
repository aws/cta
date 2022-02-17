using CTA.FeatureDetection.Common.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CTA.FeatureDetection.Tests.CTA.FeatureDetection.Common.Extensions
{
    public class SyntaxNodeExtensionsTests
    {
        //[SetUp]
        //public void Setup()
        //{ }

        [Test]
        public void RemoveCommentsFromSyntaxTree_Returns_True_If_NoComments()
        {
            const string sourceCodeWithComments = @"
                    // using Microsoft.Owin.Security;

                    /*
                     * using Microsoft.Owin.Security;
                     */

                    using System;
                    // using Microsoft.Owin.Security;

                    /*
                     * using Microsoft.Owin.Security;
                     */

                    namespace MicrosoftOwinTest
                    {
                        /*
                         * using Microsoft.Owin.Security;
                         */
                        internal class Program
                        {
                            // using Microsoft.Owin.Security;
                            static void Main(string[] args)
                            {
                                // AuthenticationDescription authentication = new AuthenticationDescription();
                            }

                            // using Microsoft.Owin.Security;
                        }

                        // using Microsoft.Owin.Security;

                        /*
                         * using Microsoft.Owin.Security;
                         */

                    }

                    /*
                     * using Microsoft.Owin.Security;
                     */

                    // using Microsoft.Owin.Security;
                    ";

            const string targetCodeWithoutTrivia = @"usingSystem;namespaceMicrosoftOwinTest{internalclassProgram{staticvoidMain(string[]args){}}}";

            SyntaxNode sourceRootWithComments = CSharpSyntaxTree.ParseText(sourceCodeWithComments).GetRoot();
            string targetCodeAfterRemoveAllTrivia = sourceRootWithComments.RemoveAllTrivia().ToFullString();

            Assert.AreEqual(targetCodeAfterRemoveAllTrivia, targetCodeWithoutTrivia);
        }
    }
}
