using System.Collections.Generic;
using CTA.Rules.Models.Tokens;

namespace CTA.Rules.Models
{
    public class RootNodes
    {
        public RootNodes()
        {
            Attributetokens = new HashSet<AttributeToken>();
            Classdeclarationtokens = new HashSet<ClassDeclarationToken>();
            ElementAccesstokens = new HashSet<ElementAccessToken>();
            Identifiernametokens = new HashSet<IdentifierNameToken>();
            Invocationexpressiontokens = new HashSet<InvocationExpressionToken>();
            Expressiontokens = new HashSet<ExpressionToken>();
            MemberAccesstokens = new HashSet<MemberAccessToken>();
            Usingdirectivetokens = new HashSet<UsingDirectiveToken>();
            MethodDeclarationTokens = new HashSet<MethodDeclarationToken>();
            ObjectCreationExpressionTokens = new HashSet<ObjectCreationExpressionToken>();
            InterfaceDeclarationTokens = new HashSet<InterfaceDeclarationToken>();
            NamespaceTokens = new HashSet<NamespaceToken>();
            ProjectTokens = new HashSet<ProjectToken>();
        }


        public HashSet<AttributeToken> Attributetokens { get; set; }
        public HashSet<ClassDeclarationToken> Classdeclarationtokens { get; set; }
        public HashSet<InterfaceDeclarationToken> InterfaceDeclarationTokens { get; set; }
        public HashSet<ElementAccessToken> ElementAccesstokens { get; set; }
        public HashSet<IdentifierNameToken> Identifiernametokens { get; set; }
        public HashSet<InvocationExpressionToken> Invocationexpressiontokens { get; set; }
        public HashSet<ExpressionToken> Expressiontokens { get; set; }
        public HashSet<MemberAccessToken> MemberAccesstokens { get; set; }
        public HashSet<UsingDirectiveToken> Usingdirectivetokens { get; set; }
        public HashSet<MethodDeclarationToken> MethodDeclarationTokens { get; set; }
        public HashSet<NamespaceToken> NamespaceTokens { get; set; }
        public HashSet<ObjectCreationExpressionToken> ObjectCreationExpressionTokens { get; set; }
        public HashSet<ProjectToken> ProjectTokens { get; set; }
    }
}
