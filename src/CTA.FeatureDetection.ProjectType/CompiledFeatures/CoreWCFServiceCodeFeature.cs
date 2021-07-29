using System;
using System.Collections.Generic;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Models.WCF;
using CTA.FeatureDetection.Common.WCFConfigUtils;

namespace CTA.FeatureDetection.ProjectType.CompiledFeatures
{
    class CoreWCFServiceCodeFeature : CompiledFeature
    {
        /// <summary>
        /// Determines if a project is a CoreWCF Compatible Code based Service based on the following :-
        ///     The Service has an interface with ServiceContract Annotation and a method 
        ///     definition with ObjectContract Annotation, and a class implementing this interface.
        ///     It should also have at least one binding and security mode compatible in CoreWCF.
        /// </summary>
        /// <param name="analyzerResult"></param>
        /// <returns>Whether a project is a CoreWCF Compatible Code based Service or not</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
           if(!IsWCFCodeService(analyzerResult))
           {
               return false;
           }

            Dictionary<string, BindingConfiguration> bindingsTransportMap = WCFBindingAndTransportUtil.GetBindingAndTransport(analyzerResult);

            return CoreWCFParityCheck.HasCoreWCFParity(bindingsTransportMap);
        }

        public bool IsWCFCodeService(AnalyzerResult analyzerResult)
        {
            // For Code Based, look for Service Interface

            var project = analyzerResult.ProjectResult;

            Tuple<string, string> serviceInterfaceAndClass = WCFBindingAndTransportUtil.GetServiceInterfaceAndClass(project);

            return serviceInterfaceAndClass != null;
        }
    }
}
