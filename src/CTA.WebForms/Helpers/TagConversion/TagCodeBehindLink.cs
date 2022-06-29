using System.Collections.Generic;
using System.Threading.Tasks;
using CTA.WebForms.TagCodeBehindHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.Helpers.TagConversion
{
    public class TagCodeBehindLink
    {
        public TaskCompletionSource<bool> HandlersStagingTaskSource { get; }
        public TaskCompletionSource<bool> ClassDeclarationRegisteredTaskSource { get; }
        public List<TagCodeBehindHandler> Handlers { get; }
        public SemanticModel SemanticModel { get; set; }
        public ClassDeclarationSyntax ClassDeclaration { get; set; }
        public bool CodeBehindFileExists { get; set; }
        public bool ViewFileExists { get; set; }

        public TagCodeBehindLink()
        {
            HandlersStagingTaskSource = new TaskCompletionSource<bool>();
            ClassDeclarationRegisteredTaskSource = new TaskCompletionSource<bool>();
            Handlers = new List<TagCodeBehindHandler>();
        }
    }
}
