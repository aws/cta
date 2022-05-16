using System.Collections.Generic;
using CTA.Rules.Models.Tokens;
using CTA.Rules.Models.Tokens.VisualBasic;
using InvocationExpressionToken = CTA.Rules.Models.Tokens.VisualBasic.InvocationExpressionToken;
using NamespaceToken = CTA.Rules.Models.Tokens.VisualBasic.NamespaceToken;
using ProjectToken = CTA.Rules.Models.Tokens.VisualBasic.ProjectToken;

namespace CTA.Rules.Models.VisualBasic
{
    public class VisualBasicRootNodes
    {
        public VisualBasicRootNodes()
        {
            InvocationExpressionTokens = new HashSet<InvocationExpressionToken>();
            ImportStatementTokens = new HashSet<ImportStatementToken>();
            NamespaceTokens = new HashSet<NamespaceToken>();
            ProjectTokens = new HashSet<NodeToken>();
        }
        
        public HashSet<InvocationExpressionToken> InvocationExpressionTokens { get; set; }
        public HashSet<ImportStatementToken> ImportStatementTokens { get; set; }
        public HashSet<NamespaceToken> NamespaceTokens { get; set; }
        public HashSet<NodeToken> ProjectTokens { get; set; }
    }
}
