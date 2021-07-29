
namespace CTA.FeatureDetection.Common.WCFConfigUtils
{
    public class Constants
    {
        internal const string SystemServiceModelElement = "system.serviceModel";
        internal const string WCFClientElement = "client";
        internal const string WCFServiceElement = "services";
        internal const string WCFServiceEndpoint = "endpoint";
        internal const string BindingElement = "element";
        internal const string ServiceContractAttribute = "ServiceContractAttribute";
        internal const string OperationContractAttribute = "OperationContractAttribute";
        internal const string BindingsAttribute = "bindings";
        internal const string BindingAttribute = "binding";
        internal const string SecurityElement = "security";
        internal const string ModeAttribute = "mode";
        internal const string ProtocolMappingAttribute = "protocolMapping";
        internal const string AddElement = "add";

        internal const string AddServiceEndpointType = "AddServiceEndpoint";
        internal const string EndpointAddressType = "EndpointAddress";
        internal const string SystemServiceModelClass = "System.ServiceModel";
        internal const string ServiceEndpointClass = "ServiceEndpoint";

        internal const string NetTcpBinding = "nettcpbinding";
        internal const string WSHttpBinding = "wshttpbinding";
        internal const string BasicHttpBinding = "basichttpbinding";
        internal const string BasicHttpsBinding = "basichttpsbinding";
        internal const string NetHttpBinding = "nethttpbinding";

        internal const string NoneMode = "None";
        internal const string TransportMode = "Transport";
        internal const string TransportWithMessageCredMode = "TransportWithMessageCredential";

        internal const string BasicHttpSecurityMode = "BasicHttpSecurityMode";
        internal const string BasicHttpsSecurityMode = "BasicHttpsSecurityMode";
        internal const string SecurityMode = "SecurityMode";

        internal const string DebuggerStepThroughAttribute = "System.Diagnostics.DebuggerStepThroughAttribute";
        internal const string GeneratedCodeAttribute = "System.CodeDom.Compiler.GeneratedCodeAttribute";

        internal const string ModeSeparator = ".";

        internal const string ConfigurationElement = "configuration";
        internal static readonly string WCFClientElementPath = $"{ConfigurationElement}/{SystemServiceModelElement}/{WCFClientElement}";
        internal static readonly string WCFServiceElementPath = $"{ConfigurationElement}/{SystemServiceModelElement}/{WCFServiceElement}";
        internal static readonly string WCFServiceEndpointElementPath = $"{ConfigurationElement}/{SystemServiceModelElement}/{WCFServiceEndpointElementPath}";
        internal static readonly string WCFBindingElementPath = $"{ConfigurationElement}/{SystemServiceModelElement}/{BindingsAttribute}";
        internal static readonly string WCFProtocolMappingElement = $"{ConfigurationElement}/{SystemServiceModelElement}/{ProtocolMappingAttribute}";
    }
}
