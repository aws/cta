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
            ObjectCreationExpressionActions = new List<ObjectCreationExpressionAction>();
            ElementAccessActions = new List<ElementAccessAction>();
            AttributeActions = new List<AttributeAction>();
            AttributeListActions = new List<AttributeAction>();
            ClassDeclarationActions = new List<ClassDeclarationAction>();
            MethodDeclarationActions = new List<MethodDeclarationAction>();
            InterfaceDeclarationActions = new List<InterfaceDeclarationAction>();
        }
        
        public List<UsingAction> UsingActions { get; set; }
        public List<InvocationExpressionAction<InvocationExpressionSyntax>> InvocationExpressionActions { get; set; }
        public List<NamespaceAction<NamespaceDeclarationSyntax>> NamespaceActions { get; set; }
        public List<IdentifierNameAction<IdentifierNameSyntax>> IdentifierNameActions { get; set; }
        public List<ObjectCreationExpressionAction> ObjectCreationExpressionActions{ get; set; }
        public List<ElementAccessAction> ElementAccessActions { get; set; }
        public List<AttributeAction> AttributeActions { get; set; }
        public List<AttributeAction> AttributeListActions { get; set; }
        public List<ClassDeclarationAction> ClassDeclarationActions { get; set; }
        public List<InterfaceDeclarationAction> InterfaceDeclarationActions { get; set; }
        public List<MethodDeclarationAction> MethodDeclarationActions { get; set; }

        public override CsharpNodeToken Clone()
        {
            CsharpNodeToken cloned = (CsharpNodeToken)base.Clone();
            cloned.UsingActions = cloned.UsingActions.Select(action => action.Clone<UsingAction>()).ToList();
            cloned.IdentifierNameActions = cloned.IdentifierNameActions.Select(action => action.Clone<IdentifierNameAction<IdentifierNameSyntax>>()).ToList();
            cloned.InvocationExpressionActions = cloned.InvocationExpressionActions.Select(action => action.Clone<InvocationExpressionAction<InvocationExpressionSyntax>>()).ToList();
            cloned.NamespaceActions = cloned.NamespaceActions.Select(action => action.Clone<NamespaceAction<NamespaceDeclarationSyntax>>()).ToList();
            cloned.ObjectCreationExpressionActions = cloned.ObjectCreationExpressionActions.Select(action => action.Clone<ObjectCreationExpressionAction>()).ToList();
            cloned.ElementAccessActions = cloned.ElementAccessActions.Select(action => action.Clone<ElementAccessAction>()).ToList();
            cloned.AttributeActions = cloned.AttributeActions?.Select(action => action.Clone<AttributeAction>())?.ToList();
            cloned.AttributeListActions = cloned.AttributeListActions?.Select(action => action.Clone<AttributeAction>())?.ToList();
            cloned.ClassDeclarationActions = cloned.ClassDeclarationActions?.Select(action => action.Clone<ClassDeclarationAction>())?.ToList();
            cloned.InterfaceDeclarationActions = cloned.InterfaceDeclarationActions.Select(action => action.Clone<InterfaceDeclarationAction>()).ToList();
            cloned.MethodDeclarationActions = cloned.MethodDeclarationActions.Select(action => action.Clone<MethodDeclarationAction>()).ToList();

            return cloned;
        }

        public override List<GenericAction> AllActions
        {
            get
            {
                var allActions = new List<GenericAction>();
                allActions.AddRange(AttributeActions);
                allActions.AddRange(AttributeListActions);
                allActions.AddRange(InvocationExpressionActions);
                allActions.AddRange(UsingActions);
                allActions.AddRange(NamespaceActions);
                allActions.AddRange(IdentifierNameActions);
                allActions.AddRange(ObjectCreationExpressionActions);
                allActions.AddRange(ElementAccessActions);
                allActions.AddRange(MethodDeclarationActions);
                allActions.AddRange(ClassDeclarationActions);
                allActions.AddRange(InterfaceDeclarationActions);
                allActions.AddRange(base.AllActions);
                return allActions;
            }
        }

    }
}
