using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CTA.WebForms2Blazor.ControlConverters;
using CTA.WebForms2Blazor.FileInformationModel;
using CTA.WebForms2Blazor.Helpers;
using CTA.WebForms2Blazor.Helpers.ControlHelpers;
using CTA.WebForms2Blazor.Metrics;
using CTA.WebForms2Blazor.Services;
using HtmlAgilityPack;

namespace CTA.WebForms2Blazor.FileConverters
{
    public class ViewFileConverter : FileConverter
    {
        private const string ChildActionType = "ViewFileConverter";
        private const string UnSupportedControlConverter = "UnSupportedControlConverter";
        private ViewImportService _viewImportService;
        private List<ControlConversionAction> ControlActions;
        private readonly WebFormMetricContext _metricsContext;
        
        public ViewFileConverter(
            string sourceProjectPath,
            string fullPath,
            ViewImportService viewImportService,
            TaskManagerService taskManagerService,
            WebFormMetricContext metricsContext) 
            : base(sourceProjectPath, fullPath, taskManagerService)
        {
            _viewImportService = viewImportService;
            ControlActions = new List<ControlConversionAction>();
            _metricsContext = metricsContext;
        }

        private HtmlDocument GetRazorContents(string htmlString)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlString);
            
            //This ensures that the document will output the original case when called by .WriteTo()
            //otherwise, all nodes and attribute names will be in lowercase
            htmlDoc.OptionOutputOriginalCase = true;
            
            FindConversionActions(htmlDoc.DocumentNode, null);
            
            // This will modify the HtmlDocument nodes that will then be changed to a file information object
            ConvertNodes();

            return htmlDoc;
        }

        // Performs a DFS traversal of the HTML tree, adding nodes to be converted in postorder
        private void FindConversionActions(HtmlNode node, HtmlNode parent)
        {
            if (node == null)
            {
                return;
            }

            foreach (HtmlNode child in node.ChildNodes)
            {
                FindConversionActions(child, node);
            }

            GetActions(node, parent);
        }

        private void GetActions(HtmlNode node, HtmlNode parent)
        {
            string controlConverterType = "";
            if (SupportedControls.ControlRulesMap.ContainsKey(node.Name))
            {
                var conversionAction = new ControlConversionAction(node, parent, SupportedControls.ControlRulesMap[node.Name]);
                controlConverterType = conversionAction.Rules.GetType().Name;
                ControlActions.Add(conversionAction);
            } 
            else if (SupportedControls.UserControls.UserControlRulesMap.ContainsKey(node.Name))
            {
                var conversionAction = new ControlConversionAction(node, parent, SupportedControls.UserControls.UserControlRulesMap[node.Name]);
                controlConverterType = conversionAction.Rules.GetType().Name;
                ControlActions.Add(conversionAction);
            }
            else if(UnknownControlRemover.ControlStartTagRegex.IsMatch(node.Name) || UnknownControlRemover.ControlStartTagRegex.IsMatch(node.Name))
            {
                
                controlConverterType = UnSupportedControlConverter;
            }
            _metricsContext.CollectControlConversionMetrics(controlConverterType, node.Name);
        }

        private void ConvertNodes()
        {
            foreach (var package in ControlActions)
            {
                HtmlNode convertedNode = package.Rules.Convert2Blazor(package.Node);
                if (convertedNode != null)
                {
                    package.Parent.ReplaceChild(convertedNode, package.Node);
                }
            }
        }
        
        // View file converters will return razor file contents with
        // only view layer, code behind will be created in another file
        public override Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();
            _metricsContext.CollectFileConversionMetrics(ChildActionType);

            string htmlString = File.ReadAllText(FullPath);
            
            //Replace directives first to build up list of user controls to be converted later by the ControlConverters
            string projectName = Path.GetFileName(ProjectPath);
            htmlString = EmbeddedCodeReplacers.ReplaceDirectives(htmlString, RelativePath, projectName, _viewImportService, _metricsContext);

            //Convert the Web Forms controls to Blazor equivalent
            HtmlDocument migratedDocument = GetRazorContents(htmlString);
            string contents = migratedDocument.DocumentNode.WriteTo();
            
            // We comment out the unknown user controls here instead of during
            // traversal because the post-order nature may comment out controls
            // that are migrated as part of an ancestor control before that ancestor
            // can be processed
            contents = ControlConverter.ConvertEmbeddedCode(contents, RelativePath, _viewImportService);
            contents = UnknownControlRemover.RemoveUnknownTags(contents);

            // Currently just changing extension to .razor, keeping filename and directory the same
            // but Razor files are renamed and moved around, can't always use same filename/directory in the future
            string newRelativePath = FilePathHelper.AlterFileName(RelativePath, newExtension: ".razor");

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
                DoCleanUp();
                LogEnd();

                // Stuff like Global.asax shouldn't create a Global.razor file
                return Task.FromResult(Enumerable.Empty<FileInformation>());
            }

            DoCleanUp();
            LogEnd();

            var fileInfo = new FileInformation(FilePathHelper.RemoveDuplicateDirectories(newRelativePath), Encoding.UTF8.GetBytes(contents));
            var result = new[] { fileInfo };

            return Task.FromResult((IEnumerable<FileInformation>)result);
        }
    }
}
