using System.Linq;
using System.Collections.Generic;
using CTA.Rules.Models.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using CTA.Rules.Models.Actions.VisualBasic;

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
            MethodBlockActions = new List<MethodBlockAction>();
            InterfaceBlockActions = new List<InterfaceBlockAction>();
            VbAttributeListActions = new List<AttributeListAction>();
            AccessorBlockActions = new List<AccessorBlockAction>();
            ObjectCreationExpressionActions = new List<CTA.Rules.Models.VisualBasic.ObjectCreationExpressionAction>();
        }

        public List<InvocationExpressionAction<InvocationExpressionSyntax>> InvocationExpressionActions { get; set; }
        public List<ImportAction> ImportActions { get; set; }
        public List<NamespaceAction<NamespaceBlockSyntax>> NamespaceActions { get; set; }
        public List<IdentifierNameAction<IdentifierNameSyntax>> IdentifierNameActions { get; set; }
        public List<TypeBlockAction> TypeBlockActions { get; set; }
        public List<MethodBlockAction> MethodBlockActions { get; set; }
        public List<InterfaceBlockAction> InterfaceBlockActions { get; set; }
        public List<AttributeListAction> VbAttributeListActions { get; set; }
        public List<AccessorBlockAction> AccessorBlockActions { get; set; }
        public List<CTA.Rules.Models.VisualBasic.ObjectCreationExpressionAction> ObjectCreationExpressionActions{ get; set; }


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
            cloned.MethodBlockActions = cloned.MethodBlockActions
                .Select(action => action.Clone<MethodBlockAction>()).ToList();
            cloned.InterfaceBlockActions = cloned.InterfaceBlockActions
                .Select(action => action.Clone<InterfaceBlockAction>()).ToList();
            cloned.VbAttributeListActions = cloned.VbAttributeListActions
                .Select(action => action.Clone<AttributeListAction>()).ToList();
            cloned.AccessorBlockActions = cloned.AccessorBlockActions
                .Select(action => action.Clone<AccessorBlockAction>()).ToList();
            cloned.ObjectCreationExpressionActions = cloned.ObjectCreationExpressionActions
                .Select(action => action.Clone<CTA.Rules.Models.VisualBasic.ObjectCreationExpressionAction>()).ToList();
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
                allActions.AddRange(MethodBlockActions);
                allActions.AddRange(InterfaceBlockActions);
                allActions.AddRange(AttributeListActions);
                allActions.AddRange(AccessorBlockActions);
                allActions.AddRange(ObjectCreationExpressionActions);
                allActions.AddRange(base.AllActions);
                return allActions;
            }
        }

    }
}
