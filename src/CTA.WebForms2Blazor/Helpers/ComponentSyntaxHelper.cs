using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms2Blazor.Helpers
{
    public static class ComponentSyntaxHelper
    {
        public static MethodDeclarationSyntax BuildComponentLifecycleMethod(BlazorComponentLifecycleEvent lcEvent, IEnumerable<StatementSyntax> statements)
        {
            switch (lcEvent)
            {
                case BlazorComponentLifecycleEvent.SetParametersAsync:
                    return BuildSetParametersAsyncMethod(statements);
                case BlazorComponentLifecycleEvent.OnParametersSet:
                    return BuildOnParametersSetMethod(statements);
                case BlazorComponentLifecycleEvent.OnAfterRender:
                    return BuildOnAfterRenderMethod(statements);
                case BlazorComponentLifecycleEvent.Dispose:
                    return BuildDisposeMethod(statements);
                default:
                    return BuildOnInitializedMethod(statements);
            }
        }

        public static MethodDeclarationSyntax BuildSetParametersAsyncMethod(IEnumerable<StatementSyntax> statements)
        {
            throw new NotImplementedException();
        }

        public static MethodDeclarationSyntax BuildOnInitializedMethod(IEnumerable<StatementSyntax> statements)
        {
            throw new NotImplementedException();
        }

        public static MethodDeclarationSyntax BuildOnParametersSetMethod(IEnumerable<StatementSyntax> statements)
        {
            throw new NotImplementedException();
        }

        public static MethodDeclarationSyntax BuildOnAfterRenderMethod(IEnumerable<StatementSyntax> statements)
        {
            throw new NotImplementedException();
        }

        public static MethodDeclarationSyntax BuildDisposeMethod(IEnumerable<StatementSyntax> statements)
        {
            throw new NotImplementedException();
        }
    }
}
