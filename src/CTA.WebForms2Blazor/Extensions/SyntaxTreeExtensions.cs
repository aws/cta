using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace CTA.WebForms2Blazor.Extensions
{
    public static class SyntaxTreeExtensions
    {
        public static IEnumerable<TypeDeclarationSyntax> GetNamespaceLevelTypes(this SyntaxTree tree)
        {
            return tree.GetRoot().DescendantNodes(node => !(node is TypeDeclarationSyntax)).OfType<TypeDeclarationSyntax>();
        }

        public static IEnumerable<SyntaxType> UnionSyntaxNodeCollections<SyntaxType>(
            this IEnumerable<SyntaxType> nodeCollection1,
            IEnumerable<SyntaxType> nodeCollection2,
            bool overwriteCollection2Nodes = true) where SyntaxType : SyntaxNode
        {
            IEnumerable<SyntaxType> trimmedCollection;

            if (overwriteCollection2Nodes)
            {
                trimmedCollection = nodeCollection2.Where(node2 => nodeCollection1.All(node1 => !node1.IsEquivalentTo(node2, false)));
                return nodeCollection1.Concat(trimmedCollection);
            }

            trimmedCollection = nodeCollection1.Where(node1 => nodeCollection2.All(node2 => !node2.IsEquivalentTo(node1, false)));
            return trimmedCollection.Concat(nodeCollection1);
        }

        public static bool IsEventHandler(this MethodDeclarationSyntax methodDeclaration, string eventHandlerName)
        {
            var paramList = methodDeclaration.ParameterList.Parameters;
            var firstParam = paramList.FirstOrDefault();
            var lastParam = paramList.LastOrDefault();

            return paramList.Count() == 2
                // Only check the types, don't need to check names as those can change and remember to check synonymous Object type alongside object
                && (firstParam.Type.ToString().Equals(Constants.SenderParamTypeName) || firstParam.Type.ToString().Equals(Constants.SenderParamTypeNameAlternate))
                && lastParam.Type.ToString().Equals(Constants.EventArgsParamTypeName)
                && methodDeclaration.Identifier.ToString().Equals(eventHandlerName);
        }

        public static bool HasEventHandlerParameters(this MethodDeclarationSyntax methodDeclaration)
        {
            var paramList = methodDeclaration.ParameterList.Parameters;
            var firstParam = paramList.FirstOrDefault();
            var lastParam = paramList.LastOrDefault();

            return paramList.Count() == 2
                // Only check the types, don't need to check names as those can change and remember to check synonymous Object type alongside object
                && (firstParam.Type.ToString().Equals(Constants.SenderParamTypeName) || firstParam.Type.ToString().Equals(Constants.SenderParamTypeNameAlternate))
                && lastParam.Type.ToString().Equals(Constants.EventArgsParamTypeName);
        }

        public static IEnumerable<string> AsStringsByLine(this SyntaxNode node)
        {
            return node.NormalizeWhitespace().ToFullString().Split(Environment.NewLine);
        }

        public static ClassDeclarationSyntax AddBaseType(this ClassDeclarationSyntax classDeclaration, string baseTypeName)
        {
            return classDeclaration.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(baseTypeName)));
        }
    }
}
