using System.Linq;
using System.Collections.Generic;
using CTA.Rules.Config;
using CTA.Rules.Models.VisualBasic;
using Microsoft.Build.Logging.StructuredLogger;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using TextSpan = Codelyzer.Analysis.Model.TextSpan;

namespace CTA.Rules.Models.Tokens.VisualBasic
{
    public class VisualBasicNodeToken : NodeToken
    {
        public VisualBasicNodeToken()
        {
            InvocationExpressionActions = new List<InvocationExpressionAction<InvocationExpressionSyntax>>();
            ImportActions = new List<ImportAction>();
            NamespaceActions = new List<NamespaceAction<NamespaceBlockSyntax>>();
        }
        
        public List<InvocationExpressionAction<InvocationExpressionSyntax>> InvocationExpressionActions { get; set; }
        public List<ImportAction> ImportActions { get; set; }
        public List<NamespaceAction<NamespaceBlockSyntax>> NamespaceActions { get; set; }

        public VisualBasicNodeToken Clone()
        {
            VisualBasicNodeToken cloned = (VisualBasicNodeToken)this.MemberwiseClone();
            cloned.TextChanges = cloned.TextChanges?.Select(textChange => textChange.Clone()).ToList();
            cloned.TargetCPU = cloned.TargetCPU?.ToList();
            cloned.InvocationExpressionActions = cloned.InvocationExpressionActions
                .Select(action => action.Clone<InvocationExpressionAction<InvocationExpressionSyntax>>()).ToList();
            cloned.ImportActions = cloned.ImportActions
                .Select(action => action.Clone<ImportAction>()).ToList();
            cloned.NamespaceActions = cloned.NamespaceActions
                .Select(action => action.Clone<NamespaceAction<NamespaceBlockSyntax>>()).ToList();
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
                allActions.AddRange(base.AllActions);
                return allActions;
            }
        }

    }
}
