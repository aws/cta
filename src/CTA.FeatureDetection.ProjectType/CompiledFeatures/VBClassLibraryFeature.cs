using System;
using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.Rules.Config;
using Microsoft.Build.Construction;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class VBClassLibraryFeature : CompiledFeature
    {
        /// <summary>
        /// Determines if a project is an VB class library project by looking at file extensions and nuget references
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is an VB class library project or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var project = analyzerResult.ProjectResult;
            var isVBProj = project.ContainsFileWithExtension(Constants.VbClassExtension, true) && project.ProjectFilePath.EndsWith(Constants.VbProjExtension)
                            &&!(project.ContainsDependency(Constants.MvcReferenceIdentifier) && project.DeclaresClassBlocksWithBaseType(Constants.MvcControllerOriginalDefinition))
                            &&!((project.ContainsNugetDependency(Constants.WebApiNugetReferenceIdentifier) || project.ContainsDependency(Constants.WebApiReferenceIdentifier))
                            && project.DeclaresClassBlocksWithBaseType(Constants.WebApiControllerOriginalDefinition));
            var isPresent = false;
            if(isVBProj)
            {
                try
                {
                    var projectRootElement = ProjectRootElement.Open(project.ProjectFilePath);
                    var outputType = projectRootElement.Properties.FirstOrDefault(x => x.Name.Equals("OutputType",StringComparison.OrdinalIgnoreCase));
                    if (outputType != null && outputType.Value.Equals("Library", StringComparison.OrdinalIgnoreCase))
                        isPresent = true;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error processing project file {0}", project.ProjectFilePath));
                }
            }

            return isPresent;
        }
    }
}
