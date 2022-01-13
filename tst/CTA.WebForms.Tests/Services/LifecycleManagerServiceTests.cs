using CTA.WebForms.Services;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTA.WebForms.Tests.Services
{
    public class LifecycleManagerServiceTests
    {
        // Need to allow some time for newly unblocked
        // task code to run before checking state again
        // This isn't really doable without awaiting the
        // task which will never return if the task is
        // going to fail
        private const int ProcessingAllowanceDelay = 25;
        private const string TestMiddlewareNamespace = "Project.Middleware";
        private const string TestOriginClassName = "OriginClassName";
        private const string TestMiddlewareName1 = "Middleware1";
        private const string TestMiddlewareName2 = "Middleware2";
        private const string BeginRequestMethodName = "Application_BeginRequest";
        private const string InitMethodName = "Page_Init";
        private const string IncorrectMethodName = "App_Begin";
        private const string SenderParamName = "sender";
        private const string SenderParamType = "object";
        private const string EventArgsParamName = "eventArgs";
        private const string EventArgsParamType = "EventArgs";
        private const string ProcessRequestMethodName = "ProcessRequest";
        private const string ProcessRequestParamType = "HttpContext";
        private const string ProcessRequestParamName = "context";
        private const string LifecycleEventStringPrefix = "application.";
        private const string PrefixedLifecycleEvent = "application.BeginRequest";
        private const string MisspelledPrefixedLifecycleEvent = "application.BeeginRequest";

        private LifecycleManagerService _lcManager;
        private CancellationToken _token;

        [SetUp]
        public void SetUp()
        {
            _lcManager = new LifecycleManagerService();
            _token = new CancellationToken();
        }

        [Test]
        public async Task GetMiddlewarePipelineAdditions_Returns_On_All_Sources_Processed()
        {
            _lcManager.NotifyExpectedMiddlewareSource();
            _lcManager.NotifyExpectedMiddlewareSource();

            var result = _lcManager.GetMiddlewarePipelineAdditions(_token);
            await Task.Delay(ProcessingAllowanceDelay);
            Assert.False(result.IsCompleted);

            _lcManager.NotifyMiddlewareSourceProcessed();
            await Task.Delay(ProcessingAllowanceDelay);
            Assert.False(result.IsCompleted);

            _lcManager.NotifyMiddlewareSourceProcessed();
            await Task.Delay(ProcessingAllowanceDelay);
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Test]
        public async Task GetMiddlewarePipelineAdditions_Organizes_Pipeline_Addition_Statements_By_Event()
        {
            var testLambda = SyntaxFactory.ParenthesizedLambdaExpression().WithBlock(SyntaxFactory.Block());

            // Should be the second registration in the sequence
            _lcManager.RegisterMiddlewareLambda(WebFormsAppLifecycleEvent.PostAcquireRequestState, testLambda);
            // Should be the first registration in the sequence
            _lcManager.RegisterMiddlewareClass(WebFormsAppLifecycleEvent.BeginRequest, TestMiddlewareName1, TestMiddlewareNamespace, TestOriginClassName, false);
            // Should be the third registration in the sequence
            _lcManager.RegisterMiddlewareClass(WebFormsAppLifecycleEvent.EndRequest, TestMiddlewareName2, TestMiddlewareNamespace, TestOriginClassName, false);

            var expectedResults = new[]
            {
                SyntaxFactory.ParseStatement($"app.UseMiddleware<{TestMiddlewareName1}>();"),
                SyntaxFactory.ParseStatement("app.Use(()=>{});"),
                SyntaxFactory.ParseStatement($"app.UseMiddleware<{TestMiddlewareName2}>();")
            };
            var actualResults = await _lcManager.GetMiddlewarePipelineAdditions(_token);

            for (int i = 0; i < expectedResults.Length; i++)
            {
                // Use trivialess and formatless comparison
                Assert.True(expectedResults[i].IsEquivalentTo(actualResults.ElementAt(i), false));
            }
        }

        [Test]
        public async Task GetMiddlewareNamespaces_Does_Not_Return_Duplicates()
        {
            _lcManager.RegisterMiddlewareClass(WebFormsAppLifecycleEvent.BeginRequest, TestMiddlewareName1, TestMiddlewareNamespace, TestOriginClassName, false);
            _lcManager.RegisterMiddlewareClass(WebFormsAppLifecycleEvent.EndRequest, TestMiddlewareName2, TestMiddlewareNamespace, TestOriginClassName, false);

            var results = await _lcManager.GetMiddlewareNamespaces(_token);

            Assert.DoesNotThrow(() => results.Single(result => result.Equals(TestMiddlewareNamespace)));
        }

        [Test]
        public void ContentIsPreHandle_Returns_True_For_First_Pre_Handle_Events()
        {
            Assert.True(LifecycleManagerService.ContentIsPreHandle(WebFormsAppLifecycleEvent.BeginRequest));
        }

        [Test]
        public void ContentIsPreHandle_Returns_True_For_Last_Pre_Handle_Events()
        {
            Assert.True(LifecycleManagerService.ContentIsPreHandle(WebFormsAppLifecycleEvent.PreRequestHandlerExecute));
        }

        [Test]
        public void ContentIsPreHandle_Returns_False_For_First_Post_Handle_Events()
        {
            Assert.False(LifecycleManagerService.ContentIsPreHandle(WebFormsAppLifecycleEvent.PostRequestHandlerExecute));
        }

        [Test]
        public void ContentIsPreHandle_Returns_False_For_Last_Post_Handle_Events()
        {
            Assert.False(LifecycleManagerService.ContentIsPreHandle(WebFormsAppLifecycleEvent.PreSendRequestContent));
        }

        [Test]
        public void CheckMethodApplicationLifecycleHook_Returns_Correct_Lifecycle_Hook()
        {
            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(BeginRequestMethodName))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(SenderParamName)).WithType(SyntaxFactory.ParseTypeName(SenderParamType)),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventArgsParamName)).WithType(SyntaxFactory.ParseTypeName(EventArgsParamType)));

            var result = LifecycleManagerService.CheckMethodApplicationLifecycleHook(methodDeclaration);

            Assert.AreEqual(WebFormsAppLifecycleEvent.BeginRequest, result);
        }

        [Test]
        public void CheckMethodApplicationLifecycleHook_Returns_Null_For_Incorrect_Params()
        {
            var methodDeclaration = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(BeginRequestMethodName));

            var result = LifecycleManagerService.CheckMethodApplicationLifecycleHook(methodDeclaration);

            Assert.IsNull(result);
        }
        
        [Test]
        public void CheckMethodApplicationLifecycleHook_Returns_Null_For_Incorrect_Name()
        {
            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(IncorrectMethodName))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(SenderParamName)).WithType(SyntaxFactory.ParseTypeName(SenderParamType)),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventArgsParamName)).WithType(SyntaxFactory.ParseTypeName(EventArgsParamType)));

            var result = LifecycleManagerService.CheckMethodApplicationLifecycleHook(methodDeclaration);

            Assert.IsNull(result);
        }

        [Test]
        public void CheckMethodPageLifecycleHook_Returns_Correct_Lifecycle_Hook()
        {
            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(InitMethodName))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(SenderParamName)).WithType(SyntaxFactory.ParseTypeName(SenderParamType)),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventArgsParamName)).WithType(SyntaxFactory.ParseTypeName(EventArgsParamType)));

            var result = LifecycleManagerService.CheckMethodPageLifecycleHook(methodDeclaration);

            Assert.AreEqual(WebFormsPageLifecycleEvent.Init, result);
        }

        [Test]
        public void CheckMethodPageLifecycleHook_Returns_Null_For_Incorrect_Params()
        {
            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(InitMethodName));

            var result = LifecycleManagerService.CheckMethodPageLifecycleHook(methodDeclaration);

            Assert.IsNull(result);
        }

        [Test]
        public void CheckMethodPageLifecycleHook_Returns_Null_For_Incorrect_Name()
        {
            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(IncorrectMethodName))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(SenderParamName)).WithType(SyntaxFactory.ParseTypeName(SenderParamType)),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(EventArgsParamName)).WithType(SyntaxFactory.ParseTypeName(EventArgsParamType)));

            var result = LifecycleManagerService.CheckMethodPageLifecycleHook(methodDeclaration);

            Assert.IsNull(result);
        }

        [Test]
        public void GetEquivalentComponentLifecycleEvent_Returns_Correct_SetParametersAsync_Events()
        {
            Assert.AreEqual(BlazorComponentLifecycleEvent.SetParametersAsync, LifecycleManagerService.GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent.PreInit));
            Assert.AreEqual(BlazorComponentLifecycleEvent.SetParametersAsync, LifecycleManagerService.GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent.Init));
        }

        [Test]
        public void GetEquivalentComponentLifecycleEvent_Returns_Correct_OnInitialized_Events()
        {
            Assert.AreEqual(BlazorComponentLifecycleEvent.OnInitialized, LifecycleManagerService.GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent.InitComplete));
            Assert.AreEqual(BlazorComponentLifecycleEvent.OnInitialized, LifecycleManagerService.GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent.LoadComplete));
        }

        [Test]
        public void GetEquivalentComponentLifecycleEvent_Returns_Correct_OnParametersSet_Events()
        {
            Assert.AreEqual(BlazorComponentLifecycleEvent.OnParametersSet, LifecycleManagerService.GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent.PreRender));
            Assert.AreEqual(BlazorComponentLifecycleEvent.OnParametersSet, LifecycleManagerService.GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent.PreRenderComplete));
        }

        [Test]
        public void GetEquivalentComponentLifecycleEvent_Returns_Correct_OnAfterRender_Events()
        {
            Assert.AreEqual(BlazorComponentLifecycleEvent.OnAfterRender, LifecycleManagerService.GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent.SaveStateComplete));
        }

        [Test]
        public void GetEquivalentComponentLifecycleEvent_Returns_Correct_Dispose_Events()
        {
            Assert.AreEqual(BlazorComponentLifecycleEvent.Dispose, LifecycleManagerService.GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent.Unload));
        }

        [Test]
        public void IsProcessRequestMethod_Returns_True_For_ProcessRequest_Method()
        {
            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(ProcessRequestMethodName))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(ProcessRequestParamName)).WithType(SyntaxFactory.ParseTypeName(ProcessRequestParamType)));

            var result = LifecycleManagerService.IsProcessRequestMethod(methodDeclaration);

            Assert.True(result);
        }

        [Test]
        public void IsProcessRequestMethod_Returns_False_For_Incorrect_Method_Name()
        {
            var methodDeclaration = SyntaxFactory
                .MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    SyntaxFactory.Identifier(IncorrectMethodName))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(ProcessRequestParamName)).WithType(SyntaxFactory.ParseTypeName(ProcessRequestParamType)));

            var result = LifecycleManagerService.IsProcessRequestMethod(methodDeclaration);

            Assert.False(result);
        }

        [Test]
        public void CheckWebFormsLifecycleEventWithPrefix_Returns_Correct_Lifecycle_Event()
        {
            Assert.AreEqual(WebFormsAppLifecycleEvent.BeginRequest, LifecycleManagerService.CheckWebFormsLifecycleEventWithPrefix(PrefixedLifecycleEvent, LifecycleEventStringPrefix));
        }

        [Test]
        public void CheckWebFormsLifecycleEventWithPrefix_Returns_Null_On_Failed_Match()
        {
            Assert.AreEqual(null, LifecycleManagerService.CheckWebFormsLifecycleEventWithPrefix(MisspelledPrefixedLifecycleEvent, LifecycleEventStringPrefix));
        }
    }
}
