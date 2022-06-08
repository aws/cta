using System.Collections.Generic;
using CTA.Rules.Models.Actions.VisualBasic;
using CTA.Rules.Models.Tokens;
using CTA.Rules.Models.Tokens.VisualBasic;
using CTA.Rules.Models.VisualBasic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CTA.Rules.Models
{
    public class FileActions
    {
        public FileActions()
        {
            AttributeActions = new HashSet<AttributeAction>();
            ClassDeclarationActions = new HashSet<ClassDeclarationAction>();
            MethodDeclarationActions = new HashSet<MethodDeclarationAction>();
            ElementAccessActions = new HashSet<ElementAccessAction>();
            IdentifierNameActions =
                new HashSet<IdentifierNameAction<Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax>>();
            InvocationExpressionActions =
                new HashSet<InvocationExpressionAction<
                    Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>>();
            ExpressionActions = new HashSet<ExpressionAction>();
            MemberAccessActions = new HashSet<MemberAccessAction>();
            Usingactions = new HashSet<UsingAction>();
            NamespaceActions = new HashSet<NamespaceAction<NamespaceDeclarationSyntax>>();
            ObjectCreationExpressionActions = new HashSet<ObjectCreationExpressionAction>();
            PackageActions = new HashSet<PackageAction>();
            InterfaceDeclarationActions = new HashSet<InterfaceDeclarationAction>();
            NodeTokens = new List<CsharpNodeToken>();

            VbNodeTokens = new List<VisualBasicNodeToken>();
            VbInvocationExpressionActions =
                new HashSet<InvocationExpressionAction<
                    Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax>>();
            VbImportActions = new HashSet<ImportAction>();
            VbNamespaceActions =
                new HashSet<NamespaceAction<NamespaceBlockSyntax>>();
            VbTypeBlockActions = new HashSet<TypeBlockAction>();
            VbMethodBlockActions = new HashSet<MethodBlockAction>();
            VbInterfaceBlockActions = new HashSet<InterfaceBlockAction>();
            VbAttributeListActions = new HashSet<AttributeListAction>();
            VbIdentifierNameActions = new HashSet<IdentifierNameAction<Microsoft.CodeAnalysis.VisualBasic.Syntax.IdentifierNameSyntax>>();
            VbAccessorBlockActions = new HashSet<AccessorBlockAction>();
        }

        public HashSet<NamespaceAction<NamespaceBlockSyntax>> VbNamespaceActions { get; set; }
        public HashSet<ImportAction> VbImportActions { get; set; }
        public HashSet<InvocationExpressionAction<Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax>>
            VbInvocationExpressionActions
        { get; set; }
        public HashSet<IdentifierNameAction<Microsoft.CodeAnalysis.VisualBasic.Syntax.IdentifierNameSyntax>>
            VbIdentifierNameActions
        { get; set; }
        public HashSet<TypeBlockAction>
            VbTypeBlockActions
        { get; set; }
        public HashSet<MethodBlockAction> VbMethodBlockActions { get; set; }
        public HashSet<InterfaceBlockAction> VbInterfaceBlockActions { get; set; }
        public HashSet<AttributeListAction> VbAttributeListActions { get; set; }
        public HashSet<AccessorBlockAction> VbAccessorBlockActions { get; set; }
        public List<VisualBasicNodeToken> VbNodeTokens { get; set; }

        public List<CsharpNodeToken> NodeTokens { get; set; }
        public string FilePath { get; set; }
        public HashSet<AttributeAction> AttributeActions { get; set; }
        public HashSet<MethodDeclarationAction> MethodDeclarationActions { get; set; }
        public HashSet<ClassDeclarationAction> ClassDeclarationActions { get; set; }
        public HashSet<InterfaceDeclarationAction> InterfaceDeclarationActions { get; set; }
        public HashSet<ElementAccessAction> ElementAccessActions { get; set; }
        public HashSet<IdentifierNameAction<Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax>> IdentifierNameActions { get; set; }
        public HashSet<InvocationExpressionAction<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>> InvocationExpressionActions { get; set; }
        public HashSet<ExpressionAction> ExpressionActions { get; set; }
        public HashSet<MemberAccessAction> MemberAccessActions { get; set; }
        public HashSet<ObjectCreationExpressionAction> ObjectCreationExpressionActions { get; set; }
        public HashSet<UsingAction> Usingactions { get; set; }
        public HashSet<NamespaceAction<NamespaceDeclarationSyntax>> NamespaceActions { get; set; }
        public HashSet<PackageAction> PackageActions { get; set; }

        public List<GenericAction> AllActions
        {
            get
            {
                var allActions = new List<GenericAction>();
                allActions.AddRange(ClassDeclarationActions);
                allActions.AddRange(MemberAccessActions);
                allActions.AddRange(IdentifierNameActions);
                allActions.AddRange(InvocationExpressionActions);
                allActions.AddRange(ExpressionActions);
                allActions.AddRange(MemberAccessActions);
                allActions.AddRange(Usingactions);
                allActions.AddRange(ObjectCreationExpressionActions);
                allActions.AddRange(NamespaceActions);

                // visual basic actions
                allActions.AddRange(VbImportActions);
                allActions.AddRange(VbNamespaceActions);
                allActions.AddRange(VbInvocationExpressionActions);
                allActions.AddRange(VbTypeBlockActions);
                allActions.AddRange(VbMethodBlockActions);
                allActions.AddRange(VbInterfaceBlockActions);
                allActions.AddRange(VbAttributeListActions);
                allActions.AddRange(VbIdentifierNameActions);
                allActions.AddRange(VbAccessorBlockActions);
                return allActions;
            }
        }
    }
}
