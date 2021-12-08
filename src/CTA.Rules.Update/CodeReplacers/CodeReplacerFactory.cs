using System.Collections.Generic;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Build;
using CTA.Rules.Models;

namespace CTA.Rules.Update
{
    public class CodeReplacerFactory
    {
        public static CodeReplacer GetInstance(List<SourceFileBuildResult> sourceFileBuildResults, ProjectConfiguration projectConfiguration, 
            List<string> metadataReferences, AnalyzerResult analyzerResult,
            List<string> updatedFiles = null, ProjectResult projectResult = null)
        {
            var projectType = projectConfiguration.ProjectType;
            var codeReplacer = projectType switch
            {
                ProjectType.WCFCodeBasedService => new WCFCodeReplacer(sourceFileBuildResults, projectConfiguration, metadataReferences, analyzerResult, updatedFiles, projectResult),
                ProjectType.WCFConfigBasedService => new WCFCodeReplacer(sourceFileBuildResults, projectConfiguration, metadataReferences, analyzerResult, updatedFiles, projectResult),
                _ => new CodeReplacer(sourceFileBuildResults, projectConfiguration, metadataReferences, analyzerResult, updatedFiles, projectResult)
            };
            return codeReplacer;
        }
    }
}
