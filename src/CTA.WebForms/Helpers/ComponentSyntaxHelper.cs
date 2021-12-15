using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.Helpers
{
    public static class ComponentSyntaxHelper
    {
        public const string SetParametersBaseCallTemplate = "await base.SetParametersAsync({0});";
        public const string ComponentOnAfterRenderParamName = "firstRender";

        public static MethodDeclarationSyntax ConstructComponentLifecycleMethod(BlazorComponentLifecycleEvent lcEvent, IEnumerable<StatementSyntax> statements)
        {
            switch (lcEvent)
            {
                case BlazorComponentLifecycleEvent.SetParametersAsync:
                    return ConstructSetParametersAsyncMethod(statements);
                case BlazorComponentLifecycleEvent.OnParametersSet:
                    return ConstructOnParametersSetMethod(statements);
                case BlazorComponentLifecycleEvent.OnAfterRender:
                    return ConstructOnAfterRenderMethod(statements);
                case BlazorComponentLifecycleEvent.Dispose:
                    return ConstructDisposeMethod(statements);
                default:
                    return ConstructOnInitializedMethod(statements);
            }
        }

        public static MethodDeclarationSyntax ConstructSetParametersAsyncMethod(IEnumerable<StatementSyntax> statements)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(typeof(Task).Name),
                BlazorComponentLifecycleEvent.SetParametersAsync.ToString());

            method = method.AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.OverrideKeyword),
                SyntaxFactory.Token(SyntaxKind.AsyncKeyword));

            method = method.AddParameterListParameters(RuntimeInjectable.ParameterViewInjectable.AsParameter);

            // Adding the call to ComponentBase.SetParametersAsync function, technically not
            // required but doesn't hurt to add and covers more cases
            var baseCall = SyntaxFactory.ParseStatement(string.Format(SetParametersBaseCallTemplate, RuntimeInjectable.ParameterViewInjectable.ParamName));
            // Adding an extra space so that the base call is separated from other method calls
            method = method.WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements.Append(CodeSyntaxHelper.GetBlankLine()).Append(baseCall)));

            // Should be public override async Task SetParametersAsync(ParameterView parameters)
            return method;
        }

        public static MethodDeclarationSyntax ConstructOnInitializedMethod(IEnumerable<StatementSyntax> statements)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                BlazorComponentLifecycleEvent.OnInitialized.ToString());

            method = method.AddModifiers(
                SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                SyntaxFactory.Token(SyntaxKind.OverrideKeyword));

            method = method.WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));

            // Should be protected override void OnInitialized()
            return method;
        }

        public static MethodDeclarationSyntax ConstructOnParametersSetMethod(IEnumerable<StatementSyntax> statements)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                BlazorComponentLifecycleEvent.OnParametersSet.ToString());

            method = method.AddModifiers(
                SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                SyntaxFactory.Token(SyntaxKind.OverrideKeyword));

            method = method.WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));

            // Should be protected override void OnParametersSet()
            return method;
        }

        public static MethodDeclarationSyntax ConstructOnAfterRenderMethod(IEnumerable<StatementSyntax> statements)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                BlazorComponentLifecycleEvent.OnAfterRender.ToString());

            method = method.AddModifiers(
                SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                SyntaxFactory.Token(SyntaxKind.OverrideKeyword));

            method = method.AddParameterListParameters(SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(ComponentOnAfterRenderParamName))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword))));
            method = method.WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));

            // Should be protected override void OnAfterRender(bool firstRender)
            return method;
        }

        public static MethodDeclarationSyntax ConstructDisposeMethod(IEnumerable<StatementSyntax> statements)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                BlazorComponentLifecycleEvent.Dispose.ToString());

            method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            method = method.WithBody(CodeSyntaxHelper.GetStatementsAsBlock(statements));

            // Should be public void Dispose()
            return method;
        }
    }
}
