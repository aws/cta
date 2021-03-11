using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class WindowsAuthorizationRolesFeature : WebConfigFeature
    {
        /// <summary>
        /// Determines if Windows Authorization Roles are being used in a given project based on
        /// Web.config settings and attributes used in code.
        ///
        /// Qualifications:
        /// 1. Web.config uses authorization and allows specifies roles:
        ///    <configuration>
        ///      <system.web>
        ///        <authorization>
        ///          <allow roles="anyRole">
        ///          </allow>
        ///        </authorization>
        ///      </system.web>
        ///    </configuration>
        /// 
        /// 2. A method is decorated with the Authorize attribute and Roles attribute argument:
        ///    [Authorize(Roles="anyRole")]
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Windows Authorization Roles are used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return IsPresentInCode(analyzerResult) || IsPresentInConfig(analyzerResult);
        }

        private bool IsPresentInConfig(AnalyzerResult analyzerResult)
        {
            var config = LoadWebConfig(analyzerResult.ProjectResult.ProjectRootPath);
            return config.ContainsAttribute(Constants.AllowElementPath, Constants.RolesAttribute);
        }
        
        private bool IsPresentInCode(AnalyzerResult analyzerResult)
        {
            var allAttributes = analyzerResult.ProjectResult.SourceFileResults.SelectMany(r => r.AllAnnotations());
            var authorizeAttributes = allAttributes.Where(a => a.Identifier == Constants.AuthorizeMethodAttribute);

            return authorizeAttributes.SelectMany(a => a.AllAttributeArguments())
                .Any(a => a.ArgumentName == Constants.RolesAttributeArgument);
        }
    }
}
