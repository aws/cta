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
            AttributeListTokens = new HashSet<AttributeListToken>();
            AccessorBlockTokens = new HashSet<AccessorBlockToken>();
            VBMemberAccesstokens = new HashSet<VBMemberAccessToken>();
            NamespaceTokens = new HashSet<NamespaceToken>();
            ProjectTokens = new HashSet<NodeToken>();
            IdentifierNameTokens = new HashSet<IdentifierNameToken>();
            TypeBlockTokens = new HashSet<TypeBlockToken>();
            ObjectCreationExpressionTokens = new HashSet<ObjectCreationExpressionToken>();
        }
        
        public HashSet<InvocationExpressionToken> InvocationExpressionTokens { get; set; }
        public HashSet<ImportStatementToken> ImportStatementTokens { get; set; }
        public HashSet<InterfaceBlockToken> InterfaceBlockTokens { get; set; }
        public HashSet<MethodBlockToken> MethodBlockTokens { get; set; }
        public HashSet<AttributeListToken> AttributeListTokens { get; set; }
        public HashSet<AccessorBlockToken> AccessorBlockTokens { get; set; }
        public HashSet<VBMemberAccessToken> VBMemberAccesstokens { get; set; }
        public HashSet<NamespaceToken> NamespaceTokens { get; set; }
        public HashSet<NodeToken> ProjectTokens { get; set; }
        public HashSet<IdentifierNameToken> IdentifierNameTokens { get; set; }
        public HashSet<TypeBlockToken> TypeBlockTokens { get; set; }
        public HashSet<ObjectCreationExpressionToken> ObjectCreationExpressionTokens { get; set; }
    }
}
