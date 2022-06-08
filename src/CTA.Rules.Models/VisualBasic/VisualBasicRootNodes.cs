using System.Collections.Generic;
using CTA.Rules.Models.Tokens.VisualBasic;
using NodeToken = CTA.Rules.Models.Tokens.NodeToken;

namespace CTA.Rules.Models.VisualBasic
{
    public class VisualBasicRootNodes
    {
        public VisualBasicRootNodes()
        {
            InvocationExpressionTokens = new HashSet<InvocationExpressionToken>();
            ImportStatementTokens = new HashSet<ImportStatementToken>();
            InterfaceBlockTokens = new HashSet<InterfaceBlockToken>();
            MethodBlockTokens = new HashSet<MethodBlockToken>();
            AttributeTokens = new HashSet<Tokens.VisualBasic.AttributeToken>();
            AttributeListTokens = new HashSet<AttributeListToken>();
            AccessorBlockTokens = new HashSet<AccessorBlockToken>();
            MemberAccessTokens = new HashSet<Tokens.VisualBasic.MemberAccessToken>();
            NamespaceTokens = new HashSet<NamespaceToken>();
            ExpressionTokens = new HashSet<ExpressionToken>();
            ElementAccessTokens = new HashSet<Tokens.VisualBasic.ElementAccessToken>();
            ProjectTokens = new HashSet<NodeToken>();
            IdentifierNameTokens = new HashSet<IdentifierNameToken>();
            TypeBlockTokens = new HashSet<TypeBlockToken>();
            ObjectCreationExpressionTokens = new HashSet<ObjectCreationExpressionToken>();
        }
        
        public HashSet<InvocationExpressionToken> InvocationExpressionTokens { get; set; }
        public HashSet<ImportStatementToken> ImportStatementTokens { get; set; }
        public HashSet<InterfaceBlockToken> InterfaceBlockTokens { get; set; }
        public HashSet<MethodBlockToken> MethodBlockTokens { get; set; }
        public HashSet<CTA.Rules.Models.Tokens.VisualBasic.AttributeToken> AttributeTokens { get; set; }
        public HashSet<AttributeListToken> AttributeListTokens { get; set; }
        public HashSet<AccessorBlockToken> AccessorBlockTokens { get; set; }
        public HashSet<Tokens.VisualBasic.MemberAccessToken> MemberAccessTokens { get; set; }
        public HashSet<NamespaceToken> NamespaceTokens { get; set; }
        public HashSet<ExpressionToken> ExpressionTokens { get; set; }
        public HashSet<Tokens.VisualBasic.ElementAccessToken> ElementAccessTokens { get; set; }
        public HashSet<NodeToken> ProjectTokens { get; set; }
        public HashSet<IdentifierNameToken> IdentifierNameTokens { get; set; }
        public HashSet<TypeBlockToken> TypeBlockTokens { get; set; }
        public HashSet<ObjectCreationExpressionToken> ObjectCreationExpressionTokens { get; set; }
    }
}
