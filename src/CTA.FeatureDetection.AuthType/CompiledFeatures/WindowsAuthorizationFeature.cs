using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthorizationFeature : WebConfigFeature
    {
        /// <summary>
        /// Determines if Windows Authorization is being used in a given project based on
        /// Web.config settings and attributes used in code.
        ///
        /// Qualifications:
        /// 1. Web.config uses authorization:
        ///    <configuration>
        ///      <system.web>
        ///        <authorization>
        ///        </authorization>
        ///      </system.web>
        ///    </configuration>
        /// 
        /// 2. A method is decorated with the Authorize attribute with or without any arguments:
        ///    [Authorize]
        ///    [Authorize(Roles="anyRole")]
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Windows Authorization is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return IsPresentInCode(analyzerResult) || IsPresentInConfig(analyzerResult);
        }

        private bool IsPresentInConfig(AnalyzerResult analyzerResult)
        {
            var config = LoadWebConfig(analyzerResult.ProjectResult.ProjectRootPath);
            return config.ContainsElement(Constants.AuthorizationElementPath);
        }

        private bool IsPresentInCode(AnalyzerResult analyzerResult)
        {
            var allAttributes = analyzerResult.ProjectResult.SourceFileResults.SelectMany(r => r.AllAnnotations());
            return allAttributes.Any(a => a.Identifier == Constants.AuthorizeMethodAttribute);
        }
    }
}
