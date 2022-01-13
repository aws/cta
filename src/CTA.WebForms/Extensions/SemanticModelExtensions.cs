using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace CTA.WebForms.Extensions
{
    public static class SemanticModelExtensions
    {
        public static IEnumerable<string> GetNamespacesReferencedByType(this SemanticModel model, TypeDeclarationSyntax typeDeclarationNode)
        {
            // We use a set here because there's no need to maintain
            // duplicates of the referenced namespace declarations
            var namespaces = new HashSet<string>();

            var descendantNodes = typeDeclarationNode.DescendantNodes();

            // Get references required for any inheritance
            var classTypeSymbol = model.GetDeclaredSymbol(typeDeclarationNode);
            if (classTypeSymbol.BaseType != null)
            {
                namespaces.Add(classTypeSymbol.BaseType.ContainingNamespace?.ToDisplayString());
            }
            namespaces.UnionWith(classTypeSymbol.Interfaces.Select(interfaceTypeSymbol => interfaceTypeSymbol.ContainingNamespace?.ToDisplayString()));

            // Get references required for any declarations that occur
            var variableDeclarationNodes = descendantNodes.OfType<VariableDeclarationSyntax>();
            namespaces.UnionWith(variableDeclarationNodes
                // Note that we pass node.Type to GetSymbolInfo, this is because
                // VariableDeclarationSyntax can refer to multiple declarations of
                // the same type, but we are only interested in the type itself
                .SelectMany(node => GetAllPotentialSymbols(model.GetSymbolInfo(node.Type))
                .Select(symbol => symbol.ContainingNamespace?.ToDisplayString())));

            // Get references required for any object creation that occurs,
            // this is distinct from the declaration references as the namespace
            // of the initialized type may be different from the declared type
            // of the variable it is assigned to
            var objectCreationNodes = descendantNodes.OfType<ObjectCreationExpressionSyntax>();
            namespaces.UnionWith(objectCreationNodes
                .SelectMany(node => GetAllPotentialSymbols(model.GetSymbolInfo(node))
                .Select(symbol => symbol.ContainingNamespace?.ToDisplayString())));

            // Get references required for any invocations that occur
            var invocationNodes = descendantNodes.OfType<InvocationExpressionSyntax>();
            namespaces.UnionWith(invocationNodes
                .SelectMany(node => GetAllPotentialSymbols(model.GetSymbolInfo(node)))
                .Select(symbol => symbol.ContainingNamespace?.ToDisplayString()));

            // We don't need to include the namespace that the given class belongs
            // to as references within the same namespace are already accessible
            namespaces.Remove(classTypeSymbol.ContainingNamespace?.ToDisplayString());

            // TODO: Find out why null namespaces occur and maybe replace with Codelyzer usage
            // We need to remove occurrences of the global namespace, it sometimes gets added
            // when a required namespace was not found
            return namespaces.Where(@namespace => @namespace != null && !@namespace.Equals(Constants.GlobalNamespace));
        }

        public static IEnumerable<string> GetOriginalUsingNamespaces(this SemanticModel model)
        {
            return model.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>().Select(usingDirective => usingDirective.Name.ToFullString());
        }

        public static IEnumerable<INamedTypeSymbol> GetAllInheritedBaseTypes(this INamedTypeSymbol typeSymbol)
        {
            var result = new List<INamedTypeSymbol>();
            var currentType = typeSymbol.BaseType;
            
            while (currentType != null)
            {
                result.Add(currentType);
                currentType = currentType.BaseType;
            }

            return result;
        }

        private static IEnumerable<ISymbol> GetAllPotentialSymbols(SymbolInfo symbolInfo)
        {
            if (symbolInfo.Symbol != null)
            {
                // ImmutableArray is the type used by symbolInfo.CandidateSymbols so
                // we use the same type here for consistency
                return ImmutableArray.Create(symbolInfo.Symbol);
            }

            return symbolInfo.CandidateSymbols;
        }
    }
}
