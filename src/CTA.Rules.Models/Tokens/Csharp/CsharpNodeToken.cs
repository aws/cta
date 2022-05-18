using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.Rules.Models.Tokens
{
    public class CsharpNodeToken : NodeToken
    {
        public CsharpNodeToken()
        {
            UsingActions = new List<UsingAction>();
            InvocationExpressionActions = new List<InvocationExpressionAction<InvocationExpressionSyntax>>();
            NamespaceActions = new List<NamespaceAction<NamespaceDeclarationSyntax>>();
            IdentifierNameActions = new List<IdentifierNameAction<IdentifierNameSyntax>>();
        }
        
        public List<UsingAction> UsingActions { get; set; }
        public List<InvocationExpressionAction<InvocationExpressionSyntax>> InvocationExpressionActions { get; set; }
        public List<NamespaceAction<NamespaceDeclarationSyntax>> NamespaceActions { get; set; }
        public List<IdentifierNameAction<IdentifierNameSyntax>> IdentifierNameActions { get; set; }

        public override CsharpNodeToken Clone()
        {
            CsharpNodeToken cloned = (CsharpNodeToken)base.Clone();
            cloned.UsingActions = cloned.UsingActions.Select(action => action.Clone<UsingAction>()).ToList();
            cloned.IdentifierNameActions = cloned.IdentifierNameActions.Select(action => action.Clone<IdentifierNameAction<IdentifierNameSyntax>>()).ToList();
            cloned.InvocationExpressionActions = cloned.InvocationExpressionActions.Select(action => action.Clone<InvocationExpressionAction<InvocationExpressionSyntax>>()).ToList();
            cloned.NamespaceActions = cloned.NamespaceActions.Select(action => action.Clone<NamespaceAction<NamespaceDeclarationSyntax>>()).ToList();

            return cloned;
        }

        public override List<GenericAction> AllActions
        {
            get
            {
                var allActions = new List<GenericAction>();
                allActions.AddRange(InvocationExpressionActions);
                allActions.AddRange(UsingActions);
                allActions.AddRange(NamespaceActions);
                allActions.AddRange(IdentifierNameActions);
                allActions.AddRange(base.AllActions);
                return allActions;
            }
        }

    }
}
