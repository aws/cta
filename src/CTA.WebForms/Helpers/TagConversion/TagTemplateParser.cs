using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.WebForms.Extensions;
using CTA.WebForms.Services;
using CTA.WebForms.TagCodeBehindHandlers;
using HtmlAgilityPack;

namespace CTA.WebForms.Helpers.TagConversion
{
    /// <summary>
    /// Parses tag conversion templates into a usable format.
    /// </summary>
    public class TagTemplateParser
    {
        private readonly TaskManagerService _taskManagerService;
        private readonly CodeBehindReferenceLinkerService _codeBehindLinkerService;

        public const string TargetAttributeGroup = "TargetAttribute";
        public const string SourceAttributeGroup = "SourceAttribute";
        public const string Context0Group = "Context0";
        public const string Context1Group = "Context1";

        /// <summary>
        /// Regular expression that matches attribute assignments with template holders for values.
        /// 
        /// <list type="bullet">
        ///     <listheader>
        ///         <term>This will match strings following any of these general formats</term>
        ///     </listheader>
        ///     <item>
        ///         <description>{TargetAttribute}=#{SourceAttribute}#</description>
        ///     </item>
        ///     <item>
        ///         <description>{TargetAttribute}=#{SourceAttribute}:{TargetType}#</description>
        ///     </item>
        ///     <item>
        ///         <description>{TargetAttribute}=#{SourceAttribute}:{CodeBehindName}:{TargetType}#</description>
        ///     </item>
        /// </list>
        ///     
        /// Spaces are allowed and ignored adjacent to separating characters in the pattern such
        /// as "=", ":", and "#" but nowhere else.
        /// </summary>
        public static Regex AttributeReplacementRegex =>
            new Regex(@"(?<TargetAttribute>[@\w\-]+)\s*=\s*\#\s*(?<SourceAttribute>[^:\#\s]+)(?:\s*:\s*(?<Context0>[^:\#\s]+))?(?:\s*:\s*(?<Context1>[^:\#\s]+))?\s*\#");
        /// <summary>
        /// Regular expression that matches only template placeholders. Note that this regular
        /// expression will match the the tail end of any matches made by
        /// <see cref="AttributeReplacementRegex"/>.
        /// 
        /// <list type="bullet">
        ///     <listheader>
        ///         <term>This will match strings following any of these general formats</term>
        ///     </listheader>
        ///     <item>
        ///         <description>#{SourceAttribute}#</description>
        ///     </item>
        ///     <item>
        ///         <description>#{SourceAttribute}:{TargetType}#</description>
        ///     </item>
        ///     <item>
        ///         <description>#{SourceAttribute}:{CodeBehindName}:{TargetType}#</description>
        ///     </item>
        /// </list>
        /// 
        /// Spaces are allowed and ignored adjacent to separating characters in the pattern such
        /// as ":" and "#" but nowhere else.
        /// </summary>
        public static Regex BasicReplacementRegex =>
            new Regex(@"\#\s*(?<SourceAttribute>[^:\#\s]+)(?:\s*:\s*(?<Context0>[^:\#\s]+))?(?:\s*:\s*(?<Context1>[^:\#\s]+))?\s*\#");

        /// <summary>
        /// Initializes a new instance of <see cref="TagTemplateParser"./>
        /// </summary>
        /// <param name="taskManagerService">The service instance that is used
        /// to perform managed calls to other services.</param>
        /// <param name="codeBehindLinkerService">The service instance that will
        /// be used to convert tag code behind references.</param>
        public TagTemplateParser(
            TaskManagerService taskManagerService,
            CodeBehindReferenceLinkerService codeBehindLinkerService)
        {
            _taskManagerService = taskManagerService;
            _codeBehindLinkerService = codeBehindLinkerService;
        }

        /// <summary>
        /// Used to populate a template string using the html node that its contents
        /// will replace.
        /// </summary>
        /// <param name="template">The template string to be used.</param>
        /// <param name="node">The html node that will be replaced.</param>
        /// <param name="viewFilePath">The path of the view file being modified.</param>
        /// <param name="handler">The code behind handler to be used on this node, if one exists.</param>
        /// <param name="taskId">The task id of the view file converter that called this method.</param>
        /// <returns>The fully populated template.</returns>
        public async Task<string> ParseTemplate(
            string template,
            HtmlNode node,
            string viewFilePath,
            TagCodeBehindHandler handler,
            int taskId)
        {
            var result = template;

            result = await AttributeReplacementRegex.ReplaceAsync(result,
                (Match m) => HandleAttributeReplacement(m, node, viewFilePath, handler, taskId));
            result = await BasicReplacementRegex.ReplaceAsync(result,
                (Match m) => HandleBasicReplacement(m, node, viewFilePath, handler, taskId));

            return result;
        }

        /// <summary>
        /// Converts matches made by <see cref="AttributeReplacementRegex"/>
        /// into a fully populated version or an empty string if population is not possible.
        /// </summary>
        /// <param name="m">The match to be converted.</param>
        /// <param name="node">The html node that is being replaced.</param>
        /// <param name="viewFilePath">The path of the view file being modified.</param>
        /// <param name="handler">The code behind handler to be used on this node, if one exists.</param>
        /// <param name="taskId">The task id of the view file converter that called this method.</param>
        /// <returns>The fully populated match string, if population is possible. Otherwise,
        /// an empty string.</returns>
        private async Task<string> HandleAttributeReplacement(
            Match m,
            HtmlNode node,
            string viewFilePath,
            TagCodeBehindHandler handler,
            int taskId)
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

            return await GetReplacementText(
                node,
                placeHolderValues.sourceAttribute,
                placeHolderValues.codeBehindName,
                targetAttribute,
                placeHolderValues.targetType,
                viewFilePath,
                handler,
                taskId);
        }

        /// <summary>
        /// Converts matches made by <see cref="BasicReplacementRegex"/>
        /// into a fully populated version or an empty string if population is not possible.
        /// </summary>
        /// <param name="m">The match to be converted.</param>
        /// <param name="node">The html node that is being replaced.</param>
        /// <param name="viewFilePath">The path of the view file being modified.</param>
        /// <param name="handler">The code behind handler to be used on this node, if one exists.</param>
        /// <param name="taskId">The task id of the view file converter that called this method.</param>
        /// <returns>The fully populated match string, if population is possible. Otherwise,
        /// an empty string.</returns>
        private async Task<string> HandleBasicReplacement(
            Match m,
            HtmlNode node,
            string viewFilePath,
            TagCodeBehindHandler handler,
            int taskId)
        {
            var nullablePlaceHolderValues = ParsePlaceholder(m);

            if (!nullablePlaceHolderValues.HasValue)
            {
                return string.Empty;
            }

            var placeHolderValues = nullablePlaceHolderValues.Value;

            return await GetReplacementText(
                node,
                placeHolderValues.sourceAttribute,
                placeHolderValues.codeBehindName,
                null,
                placeHolderValues.targetType,
                viewFilePath,
                handler,
                taskId);
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
        /// <param name="viewFilePath">The path of the view file being modified.</param>
        /// <param name="handler">The code behind handler to be used on this node, if one exists.</param>
        /// <param name="taskId">The task id of the view file converter that called this method.</param>
        /// <returns>The text that the given placeholder pattern will be replaced with.</returns>
        private async Task<string> GetReplacementText(
            HtmlNode node,
            string sourceAttribute,
            string codeBehindName,
            string targetAttribute,
            string targetType,
            string viewFilePath,
            TagCodeBehindHandler handler,
            int taskId)
        {
            string convertedSourceValue = null;
            string codeBehindRefBinding = null;

            try
            {
                var sourceValue = sourceAttribute.Equals("InnerHtml", StringComparison.InvariantCultureIgnoreCase) 
                    // We specifically choose to trim only the spaces beyond and including the first and last
                    // continuous sets of new line characters, this is to prevent us from accidentally messing
                    // with inner tab distance while removing the impact of optionally placing the first line
                    // of the inner html on the next line
                    ? node.InnerHtml.Trim(' ', '\t').Trim('\r', '\n')
                    : node.Attributes.Where(attr => attr.OriginalName.Equals(sourceAttribute, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault()?.Value;

                convertedSourceValue = sourceValue == null
                    ? null
                    : TagTypeConverter.ConvertToType(sourceAttribute, sourceValue, targetAttribute, targetType);

                if (handler != null)
                {
                    codeBehindRefBinding = await _taskManagerService.ManagedRun(taskId,
                        (token) => _codeBehindLinkerService.HandleCodeBehindForAttribute(
                            viewFilePath,
                            codeBehindName,
                            convertedSourceValue,
                            targetAttribute,
                            handler,
                            token));

                    if (codeBehindRefBinding != null)
                    {
                        var x = codeBehindRefBinding;
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                LogHelper.LogError(e, string.Format(
                    Constants.CaneledServiceCallLogTemplate,
                    Rules.Config.Constants.WebFormsErrorTag,
                    GetType().Name,
                    nameof(CodeBehindReferenceLinkerService),
                    nameof(CodeBehindReferenceLinkerService.HandleCodeBehindForAttribute)));
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to perform template attribute " +
                    $"replacement for {sourceAttribute} on node {node.Name}");
            }

            return codeBehindRefBinding ?? convertedSourceValue ?? string.Empty;
        }
    }
}
