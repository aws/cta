using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using CTA.Rules.Models.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class VisualBasicNodeToken : NodeToken
    {
        public VisualBasicNodeToken()
        {
            InvocationExpressionActions = new List<InvocationExpressionAction<InvocationExpressionSyntax>>();
            ImportActions = new List<ImportAction>();
            NamespaceActions = new List<NamespaceAction<NamespaceBlockSyntax>>();
            IdentifierNameActions = new List<IdentifierNameAction<IdentifierNameSyntax>>();
            TypeBlockActions = new List<TypeBlockAction>();
            
        }
        
        public List<InvocationExpressionAction<InvocationExpressionSyntax>> InvocationExpressionActions { get; set; }
        public List<ImportAction> ImportActions { get; set; }
        public List<NamespaceAction<NamespaceBlockSyntax>> NamespaceActions { get; set; }
        public List<IdentifierNameAction<IdentifierNameSyntax>> IdentifierNameActions { get; set; }
        public List<TypeBlockAction> TypeBlockActions { get; set; }


        public override VisualBasicNodeToken Clone()
        {
            VisualBasicNodeToken cloned = (VisualBasicNodeToken)base.Clone();
            cloned.InvocationExpressionActions = cloned.InvocationExpressionActions
                .Select(action => action.Clone<InvocationExpressionAction<InvocationExpressionSyntax>>()).ToList();
            cloned.ImportActions = cloned.ImportActions
                .Select(action => action.Clone<ImportAction>()).ToList();
            cloned.NamespaceActions = cloned.NamespaceActions
                .Select(action => action.Clone<NamespaceAction<NamespaceBlockSyntax>>()).ToList();
            cloned.IdentifierNameActions = cloned.IdentifierNameActions
                .Select(action => action.Clone<IdentifierNameAction<IdentifierNameSyntax>>()).ToList();
            cloned.TypeBlockActions = cloned.TypeBlockActions
                .Select(action => action.Clone<TypeBlockAction>()).ToList();
            return cloned;
        }

        public override List<GenericAction> AllActions
        {
            get
            {
                var allActions = new List<GenericAction>();
                allActions.AddRange(InvocationExpressionActions);
                allActions.AddRange(NamespaceActions);
                allActions.AddRange(ImportActions);
                allActions.AddRange(IdentifierNameActions);
                allActions.AddRange(TypeBlockActions);
                allActions.AddRange(base.AllActions);
                return allActions;
            }
        }

    }
}
