using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.WebForms.TagCodeBehindHandlers
{
    public class DefaultTagCodeBehindHandler : ITagCodeBehindHandler
    {
        public ClassDeclarationSyntax ModifyCodeBehind(ClassDeclarationSyntax classDeclaration)
        {
            throw new NotImplementedException();
        }

        public string GetBindingIfExists(string codeBehindName)
        {
            throw new NotImplementedException();
        }
    }
}
