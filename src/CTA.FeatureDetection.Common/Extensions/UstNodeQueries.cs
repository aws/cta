using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis.Model;

namespace CTA.FeatureDetection.Common.Extensions
{
    public static class UstNodeQueries
    {
        /// <summary>
        /// Searches a syntax tree to identify all class declaration nodes with a specified base type
        /// </summary>
        /// <param name="node">Syntax tree node to start searching from</param>
        /// <param name="baseTypeOriginalDefinition">Base type to look for</param>
        /// <returns>Collection of class declaration nodes with the specified base type</returns>
        public static IEnumerable<UstNode> GetDeclaredClassesByBaseType(this UstNode node,
            string baseTypeOriginalDefinition)
            => node.AllClasses().Where(c => c.BaseTypeOriginalDefinition == baseTypeOriginalDefinition);

        /// <summary>
        /// Searches a syntax tree to identify all invocation expression nodes with a specified semantic definition
        /// </summary>
        /// <param name="node">Syntax tree node to start searching from</param>
        /// <param name="semanticOriginalDefinition">Semantic definition to look for</param>
        /// <returns>Collection of invocation expression nodes with the specified semantic definition</returns>
        public static IEnumerable<UstNode> GetInvocationExpressionsBySemanticDefinition(this UstNode node,
                string semanticOriginalDefinition)
            => node.AllInvocationExpressions().Where(i => i.SemanticOriginalDefinition == semanticOriginalDefinition);

        /// <summary>
        /// Searches a syntax tree to identify all invocation expression nodes with a specified semantic return type
        /// </summary>
        /// <param name="node">Syntax tree node to start searching from</param>
        /// <param name="semanticReturnType">Semantic return type to look for</param>
        /// <returns>Collection of invocation expression nodes with the specified semantic return type</returns>
        public static IEnumerable<UstNode> GetInvocationExpressionsBySemanticReturnType(this UstNode node,
                string semanticReturnType)
            => node.AllInvocationExpressions().Where(i => i.SemanticReturnType == semanticReturnType);

        /// <summary>
        /// Searches a syntax tree to identify all invocation expression nodes with any of the specified semantic return types
        /// </summary>
        /// <param name="node">Syntax tree node to start searching from</param>
        /// <param name="semanticReturnTypes">Semantic return types to look for</param>
        /// <returns>Collection of invocation expression nodes with any of the specified semantic return types</returns>
        public static IEnumerable<UstNode> GetInvocationExpressionsBySemanticReturnType(this UstNode node,
                IEnumerable<string> semanticReturnTypes)
            => node.AllInvocationExpressions().Where(i => semanticReturnTypes.Contains(i.SemanticReturnType));

        /// <summary>
        /// Determines if a syntax tree has any invocation expression nodes with the specified semantic definition
        /// </summary>
        /// <param name="node">Syntax tree node to start searching from</param>
        /// <param name="semanticOriginalDefinition">Semantic definition to look for</param>
        /// <returns>Whether or not an invocation expression node with the specified semantic definition exists in the syntax tree</returns>
        public static bool ContainsInvocationExpressionsWithSemanticDefinition(this UstNode node,
                string semanticOriginalDefinition)
            => node.AllInvocationExpressions().Any(i => i.SemanticOriginalDefinition == semanticOriginalDefinition);

        /// <summary>
        /// Determines if a syntax tree has any Using Directive nodes with the specified identifier
        /// </summary>
        /// <param name="node">Syntax tree node to start searching from</param>
        /// <param name="usingDirectiveIdentifier">Identifier to look for</param>
        /// <returns>Whether or not a Using Directive node with the specified identifier exists in the syntax tree</returns>
        public static bool ContainsUsingDirectiveWithIdentifier(this UstNode node, string usingDirectiveIdentifier)
            => node.AllUsingDirectives().Any(i => i.Identifier == usingDirectiveIdentifier);

        /// <summary>
        /// Determines if a syntax tree has any references to objects in a namespace.
        /// Note: This differs from a UsingDirective, i.e. an unreferenced UsingDirective
        ///       will not be returned here.
        /// </summary>
        /// <param name="node">Syntax tree node with reference list</param>
        /// <param name="referenceIdentifier">Namespace used to identify a reference</param>
        /// <returns>Whether or not a reference is being used in the syntax tree</returns>
        public static bool ContainsReference(this RootUstNode node, string referenceIdentifier)
            => node.References.Any(r => r.Namespace == referenceIdentifier);
    }
}
