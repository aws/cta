using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Codelyzer.Analysis.Model;
using CTA.Rules.Config;

namespace CTA.Rules.Models.Tokens
{
    public class NodeToken
    {
        public NodeToken()
        {
            AttributeActions = new List<AttributeAction>();
            AttributeListActions = new List<AttributeAction>();
            ClassDeclarationActions = new List<ClassDeclarationAction>();
            UsingActions = new List<UsingAction>();
            IdentifierNameActions = new List<IdentifierNameAction>();
            InvocationExpressionActions = new List<InvocationExpressionAction>();
            MethodDeclarationActions = new List<MethodDeclarationAction>();
            NamespaceActions = new List<NamespaceAction>();
            ObjectCreationExpressionActions = new List<ObjectCreationExpressionAction>();
            PackageActions = new List<PackageAction>();
            InterfaceDeclarationActions = new List<InterfaceDeclarationAction>();
            ProjectLevelActions = new List<ProjectLevelAction>();
            ProjectFileActions = new List<ProjectLevelAction>();
            TargetCPU = new List<string>();
        }
        public string Key { get; set; }
        public string TrimmedKey => !string.IsNullOrEmpty(Key) ? Utils.EscapeAllWhitespace(Key) : string.Empty;
        public string Namespace { get; set; }
        public string Type { get; set; }
        public string FullKey { get; set; }
        public TextSpan TextSpan { get; set; }
        public string Description { get; set; }
        public List<string> TargetCPU { get; set; }
        public List<AttributeAction> AttributeActions { get; set; }
        public List<AttributeAction> AttributeListActions { get; set; }
        public List<ClassDeclarationAction> ClassDeclarationActions { get; set; }
        public List<InterfaceDeclarationAction> InterfaceDeclarationActions { get; set; }
        public List<MethodDeclarationAction> MethodDeclarationActions { get; set; }
        public List<UsingAction> UsingActions { get; set; }
        public List<IdentifierNameAction> IdentifierNameActions { get; set; }
        public List<InvocationExpressionAction> InvocationExpressionActions { get; set; }
        public List<NamespaceAction> NamespaceActions { get; set; }
        public List<ObjectCreationExpressionAction> ObjectCreationExpressionActions { get; set; }
        public List<PackageAction> PackageActions { get; set; }
        public List<ProjectLevelAction> ProjectLevelActions { get; set; }
        public List<ProjectLevelAction> ProjectFileActions { get; set; }
    }
}
