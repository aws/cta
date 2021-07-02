using CTA.WebForms2Blazor.Extensions;
using CTA.WebForms2Blazor.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CTA.WebForms2Blazor.Tests.Helpers
{
    public class ComponentSyntaxHelperTests
    {
        private const string TestStatementText = "var x = 10;";
        private const string ExpectedBaseCallStatementText = "await base.SetParametersAsync(parameters);";
        private const string ExpectedSetParametersAsyncMethodSignature = "public override async Task SetParametersAsync(ParameterView parameters)";
        private const string ExpectedOnInitializedMethodSignature = "protected override void OnInitialized()";
        private const string ExpectedOnParametersSetMethodSignature = "protected override void OnParametersSet()";
        private const string ExpectedOnAfterRenderMethodSignature = "protected override void OnAfterRender(bool firstRender)";
        private const string ExpectedDisposeMethodSignature = "public void Dispose()";

        private static IEnumerable<StatementSyntax> InputStatements => new[] { SyntaxFactory.ParseStatement(TestStatementText) };

        [Test]
        public void BuildSetParametersAsyncMethod_Adds_Base_Call_After_Statements()
        {
            var method = ComponentSyntaxHelper.BuildSetParametersAsyncMethod(InputStatements);

            // We check at index 4 because we expect the following setup:
            // 0: <<Method Signature>>
            // 1: {
            // 2:     <<Test Statement>>
            // 3:     
            // 4:     <<Base Call Statement>>
            var x = method.AsStringsByLine();
            Assert.AreEqual(ExpectedBaseCallStatementText, method.AsStringsByLine().ElementAt(4).Trim());
        }

        [Test]
        public void BuildSetParametersAsyncMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.BuildSetParametersAsyncMethod(InputStatements);

            Assert.AreEqual(ExpectedSetParametersAsyncMethodSignature, method.AsStringsByLine().First());
        }
        
        [Test]
        public void BuildOnInitializedMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.BuildOnInitializedMethod(InputStatements);

            Assert.AreEqual(ExpectedOnInitializedMethodSignature, method.AsStringsByLine().First());
        }

        [Test]
        public void BuildOnParametersSetMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.BuildOnParametersSetMethod(InputStatements);

            Assert.AreEqual(ExpectedOnParametersSetMethodSignature, method.AsStringsByLine().First());
        }

        [Test]
        public void BuildOnAfterRenderMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.BuildOnAfterRenderMethod(InputStatements);

            Assert.AreEqual(ExpectedOnAfterRenderMethodSignature, method.AsStringsByLine().First());
        }

        [Test]
        public void BuildDisposeMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.BuildDisposeMethod(InputStatements);

            Assert.AreEqual(ExpectedDisposeMethodSignature, method.AsStringsByLine().First());
        }
    }
}
