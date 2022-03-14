using System;
using System.Linq;
using System.Text.RegularExpressions;
using CTA.Rules.Config;
using CTA.WebForms.Services;
using HtmlAgilityPack;

namespace CTA.WebForms.Helpers.TagConversion
{
    /// <summary>
    /// Parses tag conversion templates into a usable format.
    /// </summary>
    public class TagTemplateParser
    {
        private CodeBehindReferenceLinkerService _codeBehindLinkerService;

        public const string TargetAttributeGroup = "TargetAttribute";
        public const string SourceAttributeGroup = "SourceAttribute";
        public const string Context0Group = "Context0";
        public const string Context1Group = "Context1";

        /// <summary>
        /// Regular expression that matches attribute assignments with template replacement for values.
        /// </summary>
        public static Regex AttributeReplacementRegex =>
            new Regex(@"(?<TargetAttribute>[\w\-]+)\s*=\s*\#\s*(?<SourceAttribute>[^:\#\s]+)(?:\s*:\s*(?<Context0>[^:\#\s]+))?(?:\s*:\s*(?<Context1>[^:\#\s]+))?\s*\#");
        /// <summary>
        /// Regular expression that matches only template replacements.
        /// </summary>
        public static Regex BasicReplacementRegex =>
            new Regex(@"\#\s*(?<SourceAttribute>[^:\#\s]+)(?:\s*:\s*(?<Context0>[^:\#\s]+))?(?:\s*:\s*(?<Context1>[^:\#\s]+))?\s*\#");

        /// <summary>
        /// Initializes a new instance of <see cref="TagTemplateParser"./>
        /// </summary>
        /// <param name="codeBehindLinkerService">The service instance that will
        /// be used to convert tag code behind references.</param>
        public TagTemplateParser(CodeBehindReferenceLinkerService codeBehindLinkerService)
        {
            _codeBehindLinkerService = codeBehindLinkerService;
        }

        /// <summary>
        /// Used to populate a template string using the html node that its contents
        /// will replace.
        /// </summary>
        /// <param name="template">The template string to be used.</param>
        /// <param name="node">The html node that will be replaced.</param>
        /// <returns>The fully populated template.</returns>
        public string ParseTemplate(string template, HtmlNode node)
        {
            var result = template;

            result = AttributeReplacementRegex.Replace(result, (Match m) => HandleAttributeReplacement(m, node));
            result = BasicReplacementRegex.Replace(result, (Match m) => HandleBasicReplacement(m, node));

            return result;
        }

        /// <summary>
        /// Converts matches made by <see cref="TagTemplateParser"/>.<see cref="AttributeReplacementRegex"/>
        /// into a fully populated version or an empty string if population is not possible.
        /// </summary>
        /// <param name="m">The match to be converted.</param>
        /// <param name="node">The html node that is being replaced.</param>
        /// <returns>The fully populated match string, if population is possible. Otherwise,
        /// an empty string.</returns>
        private string HandleAttributeReplacement(Match m, HtmlNode node)
        {
            var targetAttribute = m.Groups[TargetAttributeGroup].Captures.SingleOrDefault()?.Value;

            if (targetAttribute == null)
            {
                LogHelper.LogError($"{Rules.Config.Constants.WebFormsErrorTag}Match \"{m.Value}\" does not" +
                    $"contain TargetAttribute named capture group");

                return string.Empty;
            }

            var nullablePlaceHolderValues = ParsePlaceholder(m);

            if (!nullablePlaceHolderValues.HasValue)
            {
                // No need to log error here, should have already been logged
                return string.Empty;
            }

            var placeHolderValues = nullablePlaceHolderValues.Value;

            return GetReplacementText(
                node,
                placeHolderValues.sourceAttribute,
                placeHolderValues.codeBehindName,
                targetAttribute,
                placeHolderValues.targetType);
        }

        /// <summary>
        /// Converts matches made by <see cref="TagTemplateParser"/>.<see cref="BasicReplacementRegex"/>
        /// into a fully populated version or an empty string if population is not possible.
        /// </summary>
        /// <param name="m">The match to be converted.</param>
        /// <param name="node">The html node that is being replaced.</param>
        /// <returns>The fully populated match string, if population is possible. Otherwise,
        /// an empty string.</returns>
        private string HandleBasicReplacement(Match m, HtmlNode node)
        {
            var nullablePlaceHolderValues = ParsePlaceholder(m);

            if (!nullablePlaceHolderValues.HasValue)
            {
                return string.Empty;
            }

            var placeHolderValues = nullablePlaceHolderValues.Value;

            return GetReplacementText(
                node,
                placeHolderValues.sourceAttribute,
                placeHolderValues.codeBehindName,
                null,
                placeHolderValues.targetType);
        }

        /// <summary>
        /// Parses the placeholder portion of a match (the part enclosed by #s) and returns the
        /// result as a 3-tuple.
        /// </summary>
        /// <param name="m">The match containing the placeholder.</param>
        /// <returns>A 3-tuple containing information retrieved from the placeholder text, or
        /// null if something went wrong while retrieving this information.</returns>
        private (string sourceAttribute, string codeBehindName, string targetType)? ParsePlaceholder(Match m)
        {
            var sourceAttribute = m.Groups[SourceAttributeGroup].Captures.SingleOrDefault()?.Value;
            // If code behind name not specified then it defaults to the source attribute name
            string codeBehindName = sourceAttribute;
            string targetType = "String";

            if (sourceAttribute == null)
            {
                LogHelper.LogError($"{Rules.Config.Constants.WebFormsErrorTag}Match \"{m.Value}\" does not contain SourceAttribute named capture group");

                return null;
            }

            if (m.Groups[Context0Group].Success)
            {
                // In case only context group 0 exists, assume it is target type
                // (Format: #sourceAttribute:targetType#)
                targetType = m.Groups[Context0Group].Value;
                codeBehindName = sourceAttribute;

                if (m.Groups[Context1Group].Success && targetType != null)
                {
                    // If both context group 0 and 1 exist, then assume context group 0 is
                    // actually code behind name and context group 1 is target type
                    // (Format: #sourceAttribute:codeBehindName:targetType#)
                    codeBehindName = targetType;
                    targetType = m.Groups[Context1Group].Value;
                }
            }

            return (sourceAttribute, codeBehindName, targetType);
        }

        /// <summary>
        /// Generates replacement text for a given placeholder pattern in
        /// the current template using the html node to be replaced and the
        /// information that was retrieved from the original match.
        /// </summary>
        /// <param name="node">The node to be replaced.</param>
        /// <param name="sourceAttribute">The name of the attribute whose data is being converted.</param>
        /// <param name="codeBehindName">The name of <paramref name="sourceAttribute"/> as it is used in
        /// the code behind.</param>
        /// <param name="targetAttribute">The name of the attribute that the converted result will be assigned
        /// to, if one exists</param>
        /// <param name="targetType">The type that the <paramref name="sourceAttribute"/>'s value will be
        /// converted to.</param>
        /// <returns>The text that the given placeholder pattern will be replaced with.</returns>
        private string GetReplacementText(
            HtmlNode node,
            string sourceAttribute,
            string codeBehindName,
            string targetAttribute,
            string targetType)
        {
            try
            {
                var sourceValue = sourceAttribute.Equals("InnerHtml", StringComparison.InvariantCultureIgnoreCase) 
                    ? node.InnerHtml
                    : node.Attributes.Where(attr => attr.OriginalName.Equals(sourceAttribute)).FirstOrDefault()?.Value;
                // TODO: Send targetAttribute to ConvertToType
                var convertedSourceValue = TagTypeConverter.ConvertToType(sourceAttribute, sourceValue, targetAttribute, targetType);
                // TODO: Use proper context arguments for below method to check code behind binding
                // var codeBehindRefBinding = _codeBehindLinkerService.GetBindingValueIfExists();

                // return codeBehindRefBinding ?? convertedSourceValue ?? string.Empty;

                // TODO: Remove below code when above code is uncommented
                return convertedSourceValue ?? string.Empty;
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to perform template attribute " +
                    $"replacement for {sourceAttribute} on node {node.Name}");
            }

            return string.Empty;
        }
    }
}
