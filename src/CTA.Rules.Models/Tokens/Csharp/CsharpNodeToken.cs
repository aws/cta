using System.Linq;
using System.Collections.Generic;
using CTA.Rules.Config;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TextSpan = Codelyzer.Analysis.Model.TextSpan;

namespace CTA.Rules.Models.Tokens
{
    public class CsharpNodeToken : NodeToken
    {
        public CsharpNodeToken()
        {
            UsingActions = new List<UsingAction>();
            InvocationExpressionActions = new List<InvocationExpressionAction<InvocationExpressionSyntax>>();
            NamespaceActions = new List<NamespaceAction<NamespaceDeclarationSyntax>>();
        }
        
        public List<UsingAction> UsingActions { get; set; }
        public List<InvocationExpressionAction<InvocationExpressionSyntax>> InvocationExpressionActions { get; set; }
        public List<NamespaceAction<NamespaceDeclarationSyntax>> NamespaceActions { get; set; }

        public CsharpNodeToken Clone()
        {
            CsharpNodeToken cloned = (CsharpNodeToken)this.MemberwiseClone();
            cloned.TextChanges = cloned.TextChanges?.Select(textChange => textChange.Clone()).ToList();
            cloned.TargetCPU = cloned.TargetCPU?.ToList();
            cloned.AttributeActions = cloned.AttributeActions?.Select(action => action.Clone<AttributeAction>())?.ToList();
            cloned.AttributeListActions = cloned.AttributeListActions?.Select(action => action.Clone<AttributeAction>())?.ToList();
            cloned.ClassDeclarationActions = cloned.ClassDeclarationActions?.Select(action => action.Clone<ClassDeclarationAction>())?.ToList();
            cloned.InterfaceDeclarationActions = cloned.InterfaceDeclarationActions.Select(action => action.Clone<InterfaceDeclarationAction>()).ToList();
            cloned.MethodDeclarationActions = cloned.MethodDeclarationActions.Select(action => action.Clone<MethodDeclarationAction>()).ToList();
            cloned.ElementAccessActions = cloned.ElementAccessActions.Select(action => action.Clone<ElementAccessAction>()).ToList();
            cloned.MemberAccessActions = cloned.MemberAccessActions.Select(action => action.Clone<MemberAccessAction>()).ToList();
            cloned.UsingActions = cloned.UsingActions.Select(action => action.Clone<UsingAction>()).ToList();
            cloned.IdentifierNameActions = cloned.IdentifierNameActions.Select(action => action.Clone<IdentifierNameAction>()).ToList();
            cloned.InvocationExpressionActions = cloned.InvocationExpressionActions.Select(action => action.Clone<InvocationExpressionAction<InvocationExpressionSyntax>>()).ToList();
            cloned.ExpressionActions = cloned.ExpressionActions.Select(action => action.Clone<ExpressionAction>()).ToList();
            cloned.NamespaceActions = cloned.NamespaceActions.Select(action => action.Clone<NamespaceAction<NamespaceDeclarationSyntax>>()).ToList();
            cloned.ObjectCreationExpressionActions = cloned.ObjectCreationExpressionActions.Select(action => action.Clone<ObjectCreationExpressionAction>()).ToList();
            cloned.PackageActions = cloned.PackageActions.Select(action => action.Clone()).ToList();
            cloned.ProjectLevelActions = cloned.ProjectLevelActions.Select(action => action.Clone<ProjectLevelAction>()).ToList();
            cloned.ProjectFileActions = cloned.ProjectFileActions.Select(action => action.Clone<ProjectLevelAction>()).ToList();
            cloned.ProjectTypeActions = cloned.ProjectTypeActions.Select(action => action.Clone<ProjectLevelAction>()).ToList();
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
                allActions.AddRange(base.AllActions);
                return allActions;
            }
        }

    }
}
