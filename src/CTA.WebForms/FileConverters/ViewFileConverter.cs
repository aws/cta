﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Helpers;
using CTA.WebForms.Helpers.ControlHelpers;
using CTA.WebForms.Helpers.TagConversion;
using CTA.WebForms.Metrics;
using CTA.WebForms.Services;
using CTA.WebForms.TagConverters;
using HtmlAgilityPack;

namespace CTA.WebForms.FileConverters
{
    public class ViewFileConverter : FileConverter
    {
        private const string ChildActionType = "ViewFileConverter";
        private const string UnSupportedControlConverter = "UnSupportedControlConverter";
        private readonly Regex ControlTagNameRegex = new Regex(@"\w+:\w+");
        private readonly ViewImportService _viewImportService;
        private readonly CodeBehindReferenceLinkerService _codeBehindLinkerService;
        private readonly List<TagConversionAction> _tagConversionActions;
        private readonly TagConfigParser _tagConfigParser;
        private readonly WebFormMetricContext _metricsContext;
        
        public ViewFileConverter(
            string sourceProjectPath,
            string fullPath,
            ViewImportService viewImportService,
            CodeBehindReferenceLinkerService codeBehindLinkerService,
            TaskManagerService taskManagerService,
            TagConfigParser tagConfigParser,
            WebFormMetricContext metricsContext) 
            : base(sourceProjectPath, fullPath, taskManagerService)
        {
            _viewImportService = viewImportService;
            _codeBehindLinkerService = codeBehindLinkerService;
            _tagConversionActions = new List<TagConversionAction>();
            _tagConfigParser = tagConfigParser;
            _metricsContext = metricsContext;

            _codeBehindLinkerService.RegisterViewFile(FullPath);
        }

        private async Task<HtmlDocument> GetRazorContentsAsync(string htmlString)
        {
            var htmlDoc = new HtmlDocument();

            // This ensures that the document will output the original case when called by .WriteTo()
            // otherwise, all nodes and attribute names will be in lowercase
            htmlDoc.OptionOutputOriginalCase = true;
            htmlDoc.LoadHtml(htmlString);

            // Collect valid actions to execute
            FindConversionActions(htmlDoc.DocumentNode);

            // Modify HtmlDocument nodes using actions found above
            await ConvertNodesAsync();

            _codeBehindLinkerService.NotifyAllHandlerConversionsStaged(FullPath);

            // Fix spacing issues
            Utilities.NormalizeHtmlContent(htmlDoc.DocumentNode);

            return htmlDoc;
        }

        // Performs a DFS traversal of the HTML tree, adding nodes to be converted in postorder
        private void FindConversionActions(HtmlNode node)
        {
            if (node == null)
            {
                return;
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                FindConversionActions(child);
            }

            GetActions(node);
        }

        private void GetActions(HtmlNode node)
        {
            string converterType = "NonWebFormsControl";

            var converter = _tagConfigParser.GetConfigForNode(node.Name);

            if (converter != null)
            {
                converter.Initialize(_taskManager, _codeBehindLinkerService, _viewImportService);
                converterType = converter.GetType().Name;

                var conversionAction = new TagConversionAction(node, converter);
                if (conversionAction.CodeBehindHandler != null)
                {
                    _codeBehindLinkerService.RegisterCodeBehindHandler(FullPath, conversionAction.CodeBehindHandler);
                }
                _tagConversionActions.Add(conversionAction);
            }
            // TODO: Properly do custom user control mapping between these two conditions, previously we
            // handled this case incorrectly and will need an overhaul for the new config-based conversions
            else if (ControlTagNameRegex.IsMatch(node.Name))
            {
                converterType = UnSupportedControlConverter;
            }

            _metricsContext.CollectActionMetrics(WebFormsActionType.ControlConversion, converterType, node.Name);
        }

        private async Task ConvertNodesAsync()
        {
            foreach (var tagConversionAction in _tagConversionActions)
            {
                try
                {
                    await tagConversionAction.Converter.MigrateTagAsync(
                        tagConversionAction.Node,
                        FullPath,
                        tagConversionAction.CodeBehindHandler,
                        _taskId);
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Error converting node, " +
                                          $"Converter type: {tagConversionAction?.Converter?.GetType().Name}, " +
                                          $"Node name: {tagConversionAction?.Node?.Name}");
                }
            }
        }

        // View file converters will return razor file contents with
        // only view layer, code behind will be created in another file
        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();
            _metricsContext.CollectActionMetrics(WebFormsActionType.FileConversion, ChildActionType);

            var result = new List<FileInformation>();
            try
            {
                var htmlString = File.ReadAllText(FullPath);

                // Replace directives first to build up list of user controls to be converted later by the ControlConverters
                var projectName = Path.GetFileName(ProjectPath);
                htmlString = EmbeddedCodeReplacers.ReplaceDirectives(htmlString, RelativePath, projectName, _viewImportService, _metricsContext);

                // Convert the Web Forms controls to Blazor equivalent
                var migratedDocument = await GetRazorContentsAsync(htmlString);
                var contents = migratedDocument.DocumentNode.WriteTo().Trim();

                // We comment out the unknown user controls here instead of during
                // traversal because the post-order nature may comment out controls
                // that are migrated as part of an ancestor control before that ancestor
                // can be processed
                contents = ConvertEmbeddedCode(contents);
                contents = UnknownControlRemover.RemoveUnknownTags(contents);

                // Currently just changing extension to .razor, keeping filename and directory the same
                // but Razor files are renamed and moved around, can't always use same filename/directory in the future
                var newRelativePath = FilePathHelper.AlterFileName(RelativePath, newExtension: ".razor");

                if (RelativePath.EndsWith(Constants.WebFormsPageMarkupFileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    newRelativePath = Path.Combine(Constants.RazorPageDirectoryName, newRelativePath);
                }
                else if (RelativePath.EndsWith(Constants.WebFormsMasterPageMarkupFileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    newRelativePath = Path.Combine(Constants.RazorLayoutDirectoryName, newRelativePath);
                }
                else if (RelativePath.EndsWith(Constants.WebFormsControlMarkupFileExtenion, StringComparison.InvariantCultureIgnoreCase))
                {
                    newRelativePath = Path.Combine(Constants.RazorComponentDirectoryName, newRelativePath);
                }
                else
                {
                    // Default action: file type is not supported. Set newRelativePath to null to
                    // prevent file creation.
                    newRelativePath = null;
                }

                DoCleanUp();
                LogEnd();

                if (newRelativePath != null)
                {
                    var fileInformation = new FileInformation(FilePathHelper.RemoveDuplicateDirectories(newRelativePath),
                        Encoding.UTF8.GetBytes(contents));
                    result.Add(fileInformation);
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Error migrating view file {FullPath}. A new file could not be generated.");
            }

            return result;
        }

        public static string ConvertEmbeddedCode(string htmlString)
        {
            htmlString = EmbeddedCodeReplacers.ReplaceOneWayDataBinds(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceRawExprs(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceHTMLEncodedExprs(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceAspExprs(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceAspComments(htmlString);
            htmlString = EmbeddedCodeReplacers.ReplaceEmbeddedCodeBlocks(htmlString);

            return htmlString;
        }
    }
}
