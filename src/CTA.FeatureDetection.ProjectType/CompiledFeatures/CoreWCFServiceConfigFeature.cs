using System.Collections.Generic;
using System.IO;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Models.WCF;
using CTA.FeatureDetection.Common.WCFConfigUtils;
using CTA.Rules.Common.WebConfigManagement;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    class CoreWCFServiceConfigFeature : WebConfigFeature
    {
        /// <summary>
        /// Determines if a project is a CoreWCF Compatible Config based Service based on the following :-
        ///     If it has a web.config or App.config based configuaration, and has a service tag 
        ///     in the nested configuration/system.serviceModel tag.
        ///     The service should also have atleast one binding and transport mode compatible with CoreWCF.
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is a CoreWCF Compatible Config based Service or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            if(!IsWCFConfigService(analyzerResult))
            {
                return false;
            }

            Dictionary<string, BindingConfiguration> bindingsTransportMap = WCFBindingAndTransportUtil.GetBindingAndTransport(analyzerResult);

            return CoreWCFParityCheck.HasCoreWCFParity(bindingsTransportMap);
        }

        public bool IsWCFConfigService(AnalyzerResult analyzerResult)
        {
            string projectDir = analyzerResult.ProjectResult.ProjectRootPath;

            string webConfigFile = Path.Combine(projectDir, Rules.Config.Constants.WebConfig);
            string appConfigFile = Path.Combine(projectDir, Rules.Config.Constants.AppConfig);

            // For Config based look for <services> element.
            if (File.Exists(webConfigFile) || File.Exists(appConfigFile))
            {
                var config = WebConfigManager.LoadWebConfigAsXDocument(projectDir);
                if (config.ContainsElement(Constants.WCFServiceElementPath))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
