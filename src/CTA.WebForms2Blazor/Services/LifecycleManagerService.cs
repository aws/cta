using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codelyzer.Analysis.Model;
using CTA.WebForms2Blazor.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CTA.WebForms2Blazor.Extensions;

namespace CTA.WebForms2Blazor.Services
{
    public class LifecycleManagerService
    {
        private const string TooManyMiddlewareSourcesError = "Attempted to mark a middleware source processed, but the expected number of these operations has been reached";
        private const string ApplicationLifecycleHookMethodPrefix = "Application_";
        private const string PageLifecycleHookMethodPrefix = "Page_";

        // I chose to use lists here so that we can use Add()
        private List<IRegisteredMiddleware> _registeredMiddleware;
        private List<TaskCompletionSource<bool>> _allMiddlewareRegisteredTaskSources;

        private int _expectedMiddlewareSources;
        private int _numMiddlewareSourcesProcessed;
        
        public LifecycleManagerService()
        {
            _registeredMiddleware = new List<IRegisteredMiddleware>();
            _allMiddlewareRegisteredTaskSources = new List<TaskCompletionSource<bool>>();
        }

        public void NotifyExpectedMiddlewareSource()
        {
            _expectedMiddlewareSources += 1;
        }

        public void NotifyMiddlewareSourceProcessed()
        {
            _numMiddlewareSourcesProcessed += 1;

            if (_numMiddlewareSourcesProcessed == _expectedMiddlewareSources)
            {
                _allMiddlewareRegisteredTaskSources.ForEach(taskSource =>
                {
                    // A cancelled task also counts as completed
                    if (!taskSource.Task.IsCompleted)
                    {
                        taskSource.SetResult(true);
                    }
                });
            }
            else if (_numMiddlewareSourcesProcessed > _expectedMiddlewareSources)
            {
                throw new InvalidOperationException(TooManyMiddlewareSourcesError);
            }
        }

        public void RegisterMiddlewareClass(WebFormsAppLifecycleEvent lcEvent, string newClassName, string namespaceName, string originClassName, bool wasSplit, bool isHandler)
        {
            var newMiddleware = new RegisteredMiddlewareClass(lcEvent, newClassName, namespaceName, originClassName, wasSplit, isHandler);
            _registeredMiddleware.Add(newMiddleware);
        }

        public void RegisterMiddlewareLambda(WebFormsAppLifecycleEvent lcEvent, LambdaExpressionSyntax lambdaExpression)
        {
            var newMiddleware = new RegisteredMiddlewareLambda(lcEvent, lambdaExpression);
            _registeredMiddleware.Add(newMiddleware);
        }

        public async Task<IEnumerable<StatementSyntax>> GetMiddlewarePipelineAdditions(CancellationToken token)
        {
            await WaitForAllMiddlewareRegistered(token);

            return _registeredMiddleware.OrderBy(middleware => (int)middleware.LifecycleEvent).Select(middleware => middleware.GetPipelineAdditionStatement());
        }

        public async Task<IEnumerable<string>> GetMiddlewareNamespaces(CancellationToken token)
        {
            await WaitForAllMiddlewareRegistered(token);

            return _registeredMiddleware.Where(middleware => middleware is RegisteredMiddlewareClass)
                .Select(middleware => (middleware as RegisteredMiddlewareClass).NamespaceName).Distinct();
        }

        public static bool ContentIsPreHandle(WebFormsAppLifecycleEvent lcEvent)
        {
            return ((int)lcEvent) < (int)Constants.FirstPostHandleEvent;
        }

        public static WebFormsAppLifecycleEvent? CheckMethodApplicationLifecycleHook(MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.HasEventHandlerParameters())
            {
                var methodName = methodDeclaration.Identifier.ToString();

                foreach (int value in Enum.GetValues(typeof(WebFormsAppLifecycleEvent)))
                {
                    var currentEvent = (WebFormsAppLifecycleEvent)value;

                    if (methodName.Equals(ApplicationLifecycleHookMethodPrefix + currentEvent.ToString()))
                    {
                        return currentEvent;
                    }
                }
            }

            return null;
        }

        public static WebFormsPageLifecycleEvent? CheckMethodPageLifecycleHook(MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.HasEventHandlerParameters())
            {
                var methodName = methodDeclaration.Identifier.ToString();

                foreach (int value in Enum.GetValues(typeof(WebFormsPageLifecycleEvent)))
                {
                    var currentEvent = (WebFormsPageLifecycleEvent)value;

                    if (methodName.Equals(PageLifecycleHookMethodPrefix + currentEvent.ToString()))
                    {
                        return currentEvent;
                    }
                }
            }

            return null;
        }

        public static BlazorComponentLifecycleEvent GetEquivalentComponentLifecycleEvent(WebFormsPageLifecycleEvent lcEvent)
        {
            var lcEventIndex = (int)lcEvent;

            if (lcEventIndex < (int)Constants.FirstOnInitializedEvent)
            {
                return BlazorComponentLifecycleEvent.SetParametersAsync;
            }
            else if (lcEventIndex < (int)Constants.FirstOnParametersSetEvent)
            {
                return BlazorComponentLifecycleEvent.OnInitialized;
            }
            else if (lcEventIndex < (int)Constants.FirstOnAfterRenderEvent)
            {
                return BlazorComponentLifecycleEvent.OnParametersSet;
            }
            else if (lcEventIndex < (int)Constants.FirstDisposeEvent)
            {
                return BlazorComponentLifecycleEvent.OnAfterRender;
            }
            else
            {
                return BlazorComponentLifecycleEvent.Dispose;
            }
        }

        private Task<bool> WaitForAllMiddlewareRegistered(CancellationToken token)
        {
            var source = new TaskCompletionSource<bool>();

            if (_numMiddlewareSourcesProcessed >= _expectedMiddlewareSources)
            {
                source.SetResult(true);
            }
            else
            {
                token.Register(() => source.SetCanceled());
                _allMiddlewareRegisteredTaskSources.Add(source);
            }

            return source.Task;
        }

        private interface IRegisteredMiddleware
        {
            public WebFormsAppLifecycleEvent LifecycleEvent { get; }
            public StatementSyntax GetPipelineAdditionStatement();
        }

        private class RegisteredMiddlewareClass : IRegisteredMiddleware
        {
            public WebFormsAppLifecycleEvent LifecycleEvent { get; }
            public string NewClassName { get; }
            public string NamespaceName { get; }
            public string OriginClassName { get; }
            public bool WasSplit { get; }
            public bool IsHandler { get; }

            public RegisteredMiddlewareClass(WebFormsAppLifecycleEvent lcEvent, string newClassName, string namespaceName, string originClassName, bool wasSplit, bool isHandler)
            {
                LifecycleEvent = lcEvent;
                NewClassName = newClassName;
                NamespaceName = namespaceName;
                OriginClassName = originClassName;
                WasSplit = wasSplit;
                IsHandler = isHandler;
            }

            public StatementSyntax GetPipelineAdditionStatement()
            {
                if (WasSplit)
                {
                    return MiddlewareSyntaxHelper.BuildMiddlewareRegistrationSyntax(NewClassName, LifecycleEvent.ToString(), OriginClassName);
                }

                return MiddlewareSyntaxHelper.BuildMiddlewareRegistrationSyntax(NewClassName, LifecycleEvent.ToString());
            }
        }

        private class RegisteredMiddlewareLambda : IRegisteredMiddleware
        {
            public WebFormsAppLifecycleEvent LifecycleEvent { get; }
            public LambdaExpressionSyntax MiddlewareLambda { get; }

            public RegisteredMiddlewareLambda(WebFormsAppLifecycleEvent lcEvent, LambdaExpressionSyntax lambdaExpression)
            {
                LifecycleEvent = lcEvent;
                MiddlewareLambda = lambdaExpression;
            }

            public StatementSyntax GetPipelineAdditionStatement()
            {
                return MiddlewareSyntaxHelper.BuildMiddlewareLambdaRegistrationSyntax(MiddlewareLambda, LifecycleEvent.ToString());
            }
        }
    }
}
