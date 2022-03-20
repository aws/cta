using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CTA.Rules.Config;
using HtmlAgilityPack;

namespace CTA.WebForms.Helpers
{
    public class Utilities
    {
        public const int SpacesPerLevel = 4;
        public static Regex InvalidNamespaceIdentifierCharactersRegex => new Regex(@"[^\w.]");
        public static Regex DoublePeriodRegex => new Regex(@"[.]{2,}");
        public static Regex ValidNamespaceIdentifierStart => new Regex(@"^[a-zA-Z_]");
        public static Regex UnderscoreReplaceableCharacters => new Regex(@"[- ]");

        public static string SeparateStringsWithNewLine(params string[] strings)
        {
            return strings == null
                ? string.Empty
                : string.Join(Environment.NewLine, strings);
        }

        public static string NormalizeNamespaceIdentifier(string namespaceIdentifier)
        {
            // NOTE: When creating a project with spaces or hyphens in it, each space/hyphen
            // turns into an underscore, they don't get compressed into one
            namespaceIdentifier = UnderscoreReplaceableCharacters.Replace(namespaceIdentifier, "_");
            namespaceIdentifier = InvalidNamespaceIdentifierCharactersRegex.Replace(namespaceIdentifier, string.Empty);
            namespaceIdentifier = DoublePeriodRegex.Replace(namespaceIdentifier, ".");

            var isValidStart = ValidNamespaceIdentifierStart.IsMatch(namespaceIdentifier);
            if (!isValidStart)
            {
                namespaceIdentifier = "_" + namespaceIdentifier;
            }

            return namespaceIdentifier;
        }

        /// <summary>
        /// Normalizes spacing of content of <paramref name="node"/>. It is recommended
        /// to use this on the document node as opposed to other node types.
        /// </summary>
        /// <param name="node">The node whose content should be normalized.</param>
        public static void NormalizeHtmlContent(HtmlNode node)
        {
            var normalizationQueue = new Queue<(HtmlNode node, int ancestors)>();
            var textInsertionList = new List<(HtmlNode node, string text, bool insertAfter)>();
            var baseAncestorCount = node.Ancestors()?.Count() ?? 0;

            normalizationQueue.Enqueue((node, baseAncestorCount));

            while (normalizationQueue.Any())
            {
                var queueTuple = normalizationQueue.Dequeue();

                var requiredPreviousSpaces = SpacesPerLevel * queueTuple.ancestors;
                var requiredFinalSpaces = SpacesPerLevel * Math.Max(0, queueTuple.ancestors - 1);
                var requiredPreviousText = Environment.NewLine + new string(' ', requiredPreviousSpaces);
                var requiredFinalText = Environment.NewLine + new string(' ', requiredFinalSpaces);

                RemoveSpaceTextChildren(queueTuple.node);
                HandleNonTextChildren(queueTuple.node, queueTuple.ancestors, requiredPreviousText, normalizationQueue, textInsertionList);
                HandleTextChildren(queueTuple.node, requiredPreviousText);
                HandleLastChild(queueTuple.node, requiredFinalText, textInsertionList);
            }

            PerformTextInsertions(textInsertionList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">The node currently being processed.</param>
        private static void RemoveSpaceTextChildren(HtmlNode node)
        {
            var spaceTextChildren = node.ChildNodes?
                .Where(child => child.NodeType == HtmlNodeType.Text && string.IsNullOrWhiteSpace(child.InnerHtml))
                .ToList()
                ?? Enumerable.Empty<HtmlNode>();

            foreach (var child in spaceTextChildren)
            {
                node.RemoveChild(child);
            }
        }

        /// <summary>
        /// Ensures that any child comment or element nodes are preceded by a newline and the
        /// appropriate number of spaces. For use only by <see cref="NormalizeHtmlContent(HtmlNode)"/>.
        /// </summary>
        /// <param name="node">The node currently being processed.</param>
        /// <param name="ancestors">The number of ancestors of the node being processed.</param>
        /// <param name="requiredPreviousText">The spacing text to use before children of this type.</param>
        /// <param name="normalizationQueue">The queue of nodes that still need normalization.</param>
        /// <param name="textInsertionList">The list of nodes needing preceding or following space text and their
        /// recommended space texts.</param>
        private static void HandleNonTextChildren(
            HtmlNode node,
            int ancestors,
            string requiredPreviousText,
            Queue<(HtmlNode node, int ancestors)> normalizationQueue,
            List<(HtmlNode node, string text, bool insertAfter)> textInsertionList)
        {
            var nonTextChildren = node.ChildNodes?
                .Where(child => child.NodeType == HtmlNodeType.Element || child.NodeType == HtmlNodeType.Comment)
                ?? Enumerable.Empty<HtmlNode>();

            foreach (var child in nonTextChildren)
            {
                var previousSibling = child.PreviousSibling;

                if (previousSibling == null || previousSibling.NodeType != HtmlNodeType.Text)
                {
                    textInsertionList.Add((child, requiredPreviousText, false));
                }
                else
                {
                    previousSibling.InnerHtml = previousSibling.InnerHtml.TrimEnd() + requiredPreviousText;
                }

                if (child.NodeType == HtmlNodeType.Element)
                {
                    normalizationQueue.Enqueue((child, ancestors + 1));
                }
            }
        }

        /// <summary>
        /// Ensures that any child text nodes are preceded by a newline and
        /// the appropriate number of spaces. For use only by <see cref="NormalizeHtmlContent(HtmlNode)"/>.
        /// </summary>
        /// <param name="node">The node currently being processed.</param>
        /// <param name="requiredPreviousText">The spacing text to use before children of this type.</param>
        private static void HandleTextChildren(
            HtmlNode node,
            string requiredPreviousText)
        {
            var textChildren = node.ChildNodes?
                .Where(child => child.NodeType == HtmlNodeType.Text)
                ?? Enumerable.Empty<HtmlNode>();
            
            foreach (var child in textChildren)
            {
                child.InnerHtml = requiredPreviousText + child.InnerHtml.TrimStart();
            }
        }

        /// <summary>
        /// Ensure that there is a newline and the appropriate number of
        /// spaces preceding the closing tag, but only if the current
        /// element has children. For use only by <see cref="NormalizeHtmlContent(HtmlNode)"/>.
        /// </summary>
        /// <param name="node">The node currently being processed.</param>
        /// <param name="requiredFinalText">The spacing text to use after the final child node.</param>
        /// <param name="textInsertionList">The list of nodes needing preceding or following space text and their
        /// recommended space texts.</param>
        private static void HandleLastChild(
            HtmlNode node,
            string requiredFinalText,
            List<(HtmlNode node, string text, bool insertAfter)> textInsertionList)
        {
            var lastChild = node.LastChild;

            if (lastChild != null)
            {
                if (lastChild.NodeType != HtmlNodeType.Text)
                {
                    textInsertionList.Add((lastChild, requiredFinalText, true));
                }
                else
                {
                    lastChild.InnerHtml = lastChild.InnerHtml.TrimEnd() + requiredFinalText;
                }
            }
        }

        /// <summary>
        /// Performs insertion of any new text nodes needed to normalize
        /// the html structure. For use only by <see cref="NormalizeHtmlContent(HtmlNode)"/>.
        /// </summary>
        /// <param name="textInsertionList">The list of nodes needing preceding or following space text and their
        /// recommended space texts.</param>
        private static void PerformTextInsertions(List<(HtmlNode node, string text, bool insertAfter)> textInsertionList)
        {
            foreach (var textInsertion in textInsertionList)
            {
                var textNode = HtmlNode.CreateNode(textInsertion.text);

                var nonTextNode = textInsertion.node;
                var parent = nonTextNode.ParentNode;

                if (parent == null)
                {
                    LogHelper.LogError($"{Rules.Config.Constants.WebFormsErrorTag}Could not find parent of node " +
                        $"{nonTextNode.Name} when trying to normalize html");

                    continue;
                }

                if (textInsertion.insertAfter)
                {
                    parent.InsertAfter(textNode, nonTextNode);
                }
                else
                {
                    parent.InsertBefore(textNode, nonTextNode);
                }
            }
        }
    }
}
