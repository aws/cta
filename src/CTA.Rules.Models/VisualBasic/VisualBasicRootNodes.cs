using System.Collections.Generic;
using CTA.Rules.Models.Tokens.VisualBasic;
using NamespaceToken = CTA.Rules.Models.Tokens.VisualBasic.NamespaceToken;

namespace CTA.Rules.Models.VisualBasic
{
    public class VisualBasicRootNodes
    {
        public VisualBasicRootNodes()
        {
            InvocationExpressionTokens = new HashSet<InvocationExpressionToken>();
            ImportStatementTokens = new HashSet<ImportStatementToken>();
            NamespaceTokens = new HashSet<NamespaceToken>();
        }
        
        public HashSet<InvocationExpressionToken> InvocationExpressionTokens { get; set; }
        public HashSet<ImportStatementToken> ImportStatementTokens { get; set; }
        public HashSet<NamespaceToken> NamespaceTokens { get; set; }
    }
}
