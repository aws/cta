using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Features.Base;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    public class VBWebApiFeature : CompiledFeature
    {
        /// <summary>
        /// Determines if a project is an VB web api project by looking at file extensions and nuget references
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is an VB web api project or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            var project = analyzerResult.ProjectResult;
            var isPresent = (project.ContainsFileWithExtension(Constants.VbClassExtension, true) && project.ProjectFilePath.EndsWith(Constants.VbProjExtension))
                && ((project.ContainsNugetDependency(Constants.WebApiNugetReferenceIdentifier)
                || project.ContainsDependency(Constants.WebApiReferenceIdentifier))
                && project.DeclaresClassBlocksWithBaseType(Constants.WebApiControllerOriginalDefinition));

            return isPresent;
        }
    }
}
