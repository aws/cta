using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTA.Rules.Config;
using CTA.Rules.Models;
using CTA.WebForms.ClassConverters;
using CTA.WebForms.Extensions;
using CTA.WebForms.Factories;
using CTA.WebForms.FileInformationModel;
using CTA.WebForms.Metrics;
using CTA.WebForms.ProjectManagement;
using CTA.WebForms.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace CTA.WebForms.FileConverters
{
    public class CodeFileConverter : FileConverter
    {
        private const string ChildActionType = "CodeFileConverter";
        private readonly SemanticModel _fileModel;
        private readonly SemanticModel _orignialModel;
        private readonly WorkspaceManagerService _blazorWorkspaceBuilder;
        private readonly ProjectAnalyzer _webFormsProjectAnaylzer;
        private readonly ClassConverterFactory _classConverterFactory;
        private readonly IEnumerable<ClassConverter> _classConverters;
        private WebFormMetricContext _metricsContext;

        public CodeFileConverter(
            string sourceProjectPath,
            string fullPath,
            WorkspaceManagerService blazorWorkspaceManager,
            ProjectAnalyzer webFormsProjectAnalyzer,
            ClassConverterFactory classConverterFactory,
            TaskManagerService taskManagerService,
            WebFormMetricContext metricsContext) : base(sourceProjectPath, fullPath, taskManagerService)
        {
            // May not need this anymore but not sure yet
            _blazorWorkspaceBuilder = blazorWorkspaceManager;
            _webFormsProjectAnaylzer = webFormsProjectAnalyzer;
            _classConverterFactory = classConverterFactory;
            _metricsContext = metricsContext;
            var symbolClassConverterDic = new Dictionary<string, string>();
            try
            {

                var sourcefileBuildResult = _webFormsProjectAnaylzer.AnalyzerResult.ProjectBuildResult?.SourceFileBuildResults?
                    .Single(r => r.SourceFileFullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));

                _orignialModel = sourcefileBuildResult?.SemanticModel;
                var sourceFileRelativePath = sourcefileBuildResult?.SourceFileFullPath;
                var namespaceLevelTypes = _orignialModel?.SyntaxTree?.GetNamespaceLevelTypes();
                foreach (var namespaceLevelType in namespaceLevelTypes)
                {
                    var symbol = _orignialModel?.GetDeclaredSymbol(namespaceLevelType);

                    if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedGlobalBaseClass))
                        && sourceFileRelativePath.EndsWith(Constants.ExpectedGlobalFileName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        symbolClassConverterDic.TryAdd(symbol.ToDisplayString(), "GlobalClassConverter");
                    }
                    // NOTE: The order is important from this point on, mainly because
                    // Page-derived classes are also IHttpHandler derived
                    if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedPageBaseClass))
                        && sourceFileRelativePath.EndsWith(Constants.PageCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
                    {
                        symbolClassConverterDic.TryAdd(symbol.ToDisplayString(), "PageCodeBehindClassConverter");
                    }

                    if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedControlBaseClass))
                        && sourceFileRelativePath.EndsWith(Constants.ControlCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
                    {
                        symbolClassConverterDic.TryAdd(symbol.ToDisplayString(), "ControlCodeBehindClassConverter");
                    }

                    if (symbol.GetAllInheritedBaseTypes().Any(typeSymbol => typeSymbol.Name.Equals(Constants.ExpectedMasterPageBaseClass))
                        && sourceFileRelativePath.EndsWith(Constants.MasterPageCodeBehindExtension, StringComparison.InvariantCultureIgnoreCase))
                    {
                        symbolClassConverterDic.TryAdd(symbol.ToDisplayString(), "MasterPageCodeBehindClassConverter");
                    }

                    if (symbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name.Equals(Constants.HttpHandlerInterface)))
                    {
                        symbolClassConverterDic.TryAdd(symbol.ToDisplayString(), "HttpHandlerClassConverter");
                    }

                    if (symbol.AllInterfaces.Any(interfaceSymbol => interfaceSymbol.Name.Equals(Constants.HttpModuleInterface)))
                    {
                        symbolClassConverterDic.TryAdd(symbol.ToDisplayString(), "HttpModuleClassConverter");
                    }
                    symbolClassConverterDic.TryAdd(symbol.ToDisplayString(), "UnknownClassConverter");

                }

                var oldTree = sourcefileBuildResult?.SyntaxTree;
                var oldEncoding = oldTree.Encoding;
                SyntaxTree newTree = null;
                if ( fullPath.EndsWith(".cs"))
                {
                    var languageVersion = ((Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)(sourcefileBuildResult?.SemanticModel?.SyntaxTree?.Options)).LanguageVersion;
                    var options = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);
                    newTree = CSharpSyntaxTree.ParseText(File.ReadAllText(fullPath), options).WithFilePath(fullPath);
                    
                }
                else if (fullPath.EndsWith(".vb"))
                {
                    var vblanguageVersion = ((Microsoft.CodeAnalysis.VisualBasic.VisualBasicParseOptions)(sourcefileBuildResult?.SemanticModel?.SyntaxTree?.Options)).LanguageVersion;
                    var options = VisualBasicParseOptions.Default.WithLanguageVersion(vblanguageVersion);
                    newTree = VisualBasicSyntaxTree.ParseText(File.ReadAllText(fullPath), options).WithFilePath(fullPath);
                }


                if (newTree != null && newTree != oldTree)
                {
                    var newCompilation = _orignialModel?.Compilation?.ReplaceSyntaxTree(oldTree, newTree);
                    _fileModel = newCompilation?.GetSemanticModel(newTree);
                }
                
            }
            catch (IOException e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Exception occurred when trying to reload source file  [{FullPath}] syntax tree. " +
                                      "Semantic Model will default to null.");
            }
            catch (Exception e)
            {
                LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Exception occurred when trying to retrieve semantic model for the file {FullPath}. " +
                                      "Semantic Model will default to null.");
            }

            
            if (_fileModel!= null)
            {
                _classConverters = _classConverterFactory.BuildMany(symbolClassConverterDic, RelativePath, _fileModel );
            }
            else
            {
                _classConverters = new List<ClassConverter>();
                LogHelper.LogWarning($"Semantic model was not found so class conversion cannot take place. Returning zero class converters for file: {FullPath}");
            }
        }

        public override async Task<IEnumerable<FileInformation>> MigrateFileAsync()
        {
            LogStart();
            _metricsContext.CollectActionMetrics(WebFormsActionType.FileConversion, ChildActionType);

            // Need to have ToList call here to enumerate the collection and ensure class
            // converters are running before we retire this file converter task
            var classMigrationTasks = _classConverters.Select(converter => converter.MigrateClassAsync()).ToList();

            // We want to do our cleanup now because from this point on all migration tasks
            // are done by class converters and we want to make sure that we retire the task
            // related to this file converter before we await
            DoCleanUp();

            var allMigrationTasks = Task.WhenAll(classMigrationTasks);

            try
            {
                await allMigrationTasks;
            }
            // We don't provide a reference for the thrown exception here because await auto-
            // unwraps aggregate exceptions and throws only the first encountered exception
            catch
            {
                // We access allMigrationTasks.Exception instead to provide the original AggregateException
                var allExceptions = allMigrationTasks.Exception.Flatten().InnerExceptions;

                foreach (Exception e in allExceptions)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to migrate class");
                }
            }

            var result = classMigrationTasks.Where(t => t.Status == TaskStatus.RanToCompletion).SelectMany(t => t.Result);

            LogEnd();

            return result;
        }
    }
}
