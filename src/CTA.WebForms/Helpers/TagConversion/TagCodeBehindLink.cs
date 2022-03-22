using System.Collections.Generic;
using System.Threading.Tasks;
using CTA.WebForms.TagCodeBehindHandlers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.Helpers.TagConversion
{
    public class TagCodeBehindLink
    {
        public TaskCompletionSource<bool> HandlersExecutedTaskSource { get; }
        public TaskCompletionSource<bool> ClassDeclarationRegisteredTaskSource { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; set; }

        public TagCodeBehindLink()
        {
            HandlersExecutedTaskSource = new TaskCompletionSource<bool>();
            ClassDeclarationRegisteredTaskSource = new TaskCompletionSource<bool>();
        }
    }
}
