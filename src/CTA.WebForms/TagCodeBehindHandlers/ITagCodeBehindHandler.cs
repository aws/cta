using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.TagCodeBehindHandlers
{
    public interface ITagCodeBehindHandler
    {
        public ClassDeclarationSyntax ModifyCodeBehind(ClassDeclarationSyntax classDeclaration);
        public string GetBindingIfExists(string codeBehindName);
    }
}
