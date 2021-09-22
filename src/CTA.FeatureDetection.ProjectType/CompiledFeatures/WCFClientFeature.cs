using System.IO;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.Rules.Common.WebConfigManagement;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    class WCFClientFeature : WebConfigFeature
    {
        /// <summary>
        /// Determines if a project is a WCF Client Project based on the following :-
        ///     If it has a web.config or App.config based configuaration, and has a client tag 
        ///     in the nested configuration/system.serviceModel tag.
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is a WCF Client</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            string projectDir = analyzerResult.ProjectResult.ProjectRootPath;

            string webConfigFile = Path.Combine(projectDir, Rules.Config.Constants.WebConfig);
            string appConfigFile = Path.Combine(projectDir, Rules.Config.Constants.AppConfig);

            if (File.Exists(webConfigFile))
            {
                var config = WebConfigManager.LoadWebConfigAsXDocument(projectDir);
                if (config.ContainsElement(Constants.WCFClientElementPath))
                {
                    return true;
                }
            }
            if (File.Exists(appConfigFile))
            {
                var config = WebConfigManager.LoadAppConfigAsXDocument(projectDir);
                if (config.ContainsElement(Constants.WCFClientElementPath))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
