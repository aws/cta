using System.Collections.Generic;

namespace CTA.FeatureDetection.Common.WCFConfigUtils
{
    class CoreWCFBindings
    {
        public static readonly Dictionary<string, List<string>> CORE_WCF_BINDINGS =
            new Dictionary<string, List<string>>
            {
                { Constants.NetTcpBinding, new List<string> { Constants.NoneMode.ToLower(), Constants.TransportMode.ToLower(), Constants.TransportWithMessageCredMode.ToLower() } },
                { Constants.WSHttpBinding, new List<string> { Constants.NoneMode.ToLower(), Constants.TransportMode.ToLower(), Constants.TransportWithMessageCredMode.ToLower() } },
                { Constants.BasicHttpBinding, new List<string> { Constants.NoneMode.ToLower(), Constants.TransportMode.ToLower(), Constants.TransportWithMessageCredMode.ToLower() } },
                { Constants.BasicHttpsBinding, new List<string> { Constants.NoneMode.ToLower(), Constants.TransportMode.ToLower(), Constants.TransportWithMessageCredMode.ToLower() } },
                { Constants.NetHttpBinding, new List<string> { Constants.NoneMode.ToLower(), Constants.TransportMode.ToLower(), Constants.TransportWithMessageCredMode.ToLower() } }
            };
    }
}
