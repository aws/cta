using System;
using System.Collections.Generic;
using System.Linq;
using CTA.FeatureDetection.Common.Models.WCF;

namespace CTA.FeatureDetection.Common.WCFConfigUtils
{
    public class CoreWCFParityCheck
    {
        /// <summary>
        /// Check if given Transport and Mode Collection has Parity with CoreWCF.
        /// If one binding and transport has support in CoreWCF, the method returns true.
        /// </summary>
        /// <param name="bindingsTransportMap">Dictionary with Binding Name as key and list of modes as value</param>
        /// <returns>If the given bindings and transport mode has WCF parity.</returns>
        public static bool HasCoreWCFParity(Dictionary<string, BindingConfiguration> bindingsTransportMap)
        {
            bool hasCoreWCFSupport = false;

            foreach (var binding in bindingsTransportMap)
            {
                var bindingName = binding.Key;

                //Variables assigned but not used, can be used as a metric
                var unsupportedBindings = new List<string>();
                var unsupportedModes = new Dictionary<string, string>();

                if (CoreWCFBindings.CORE_WCF_BINDINGS.Keys.Contains(bindingName))
                {
                    var mode = bindingsTransportMap[bindingName].Mode;
                    var supportedModes = CoreWCFBindings.CORE_WCF_BINDINGS[bindingName];

                    if (!supportedModes.Contains(mode.ToLower()))
                    {
                        unsupportedModes.Add(bindingName, mode);
                    }

                    //If even one transport with mode is supported on CoreWCF set the flag.
                    else
                    {
                        hasCoreWCFSupport = true;
                    }
                }
                else
                {
                    unsupportedBindings.Add(bindingName);
                }
            }
            return hasCoreWCFSupport;
        }
    }
}
