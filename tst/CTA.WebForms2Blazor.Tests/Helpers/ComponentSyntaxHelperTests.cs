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
        public void ConstructSetParametersAsyncMethod_Adds_Base_Call_After_Statements()
        {
            var method = ComponentSyntaxHelper.ConstructSetParametersAsyncMethod(InputStatements);

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
        public void ConstructSetParametersAsyncMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.ConstructSetParametersAsyncMethod(InputStatements);

            Assert.AreEqual(ExpectedSetParametersAsyncMethodSignature, method.AsStringsByLine().First());
        }
        
        [Test]
        public void ConstructOnInitializedMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.ConstructOnInitializedMethod(InputStatements);

            Assert.AreEqual(ExpectedOnInitializedMethodSignature, method.AsStringsByLine().First());
        }

        [Test]
        public void ConstructOnParametersSetMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.ConstructOnParametersSetMethod(InputStatements);

            Assert.AreEqual(ExpectedOnParametersSetMethodSignature, method.AsStringsByLine().First());
        }

        [Test]
        public void ConstructOnAfterRenderMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.ConstructOnAfterRenderMethod(InputStatements);

            Assert.AreEqual(ExpectedOnAfterRenderMethodSignature, method.AsStringsByLine().First());
        }

        [Test]
        public void ConstructDisposeMethod_Creates_Correct_Method_Signature()
        {
            var method = ComponentSyntaxHelper.ConstructDisposeMethod(InputStatements);

            Assert.AreEqual(ExpectedDisposeMethodSignature, method.AsStringsByLine().First());
        }
    }
}
