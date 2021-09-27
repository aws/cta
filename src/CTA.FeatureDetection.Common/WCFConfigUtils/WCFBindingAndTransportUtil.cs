using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.WCF;
using CTA.Rules.Common.WebConfigManagement;

namespace CTA.FeatureDetection.Common.WCFConfigUtils
{
    public class WCFBindingAndTransportUtil
    {
        /// <summary>
        /// Get a Dictionary of bindings and transport modes from the Analyzer Result.
        /// </summary>
        /// <param name="analyzerResult">Codelyzer Analysis object</param>
        /// <returns>Dictionary with binding name as key and BindingConfiguration as value</returns>
        public static Dictionary<string, BindingConfiguration> GetBindingAndTransport(AnalyzerResult analyzerResult)
        {
            string projectDir = analyzerResult.ProjectResult.ProjectRootPath;

            string webConfigFile = Path.Combine(projectDir, Rules.Config.Constants.WebConfig);
            string appConfigFile = Path.Combine(projectDir, Rules.Config.Constants.AppConfig);

            var bindingsTransportMap = new Dictionary<string, BindingConfiguration>();

            if (File.Exists(webConfigFile) || File.Exists(appConfigFile))
            {
                ConfigBasedCheck(projectDir, bindingsTransportMap);
            }

            var projectWorkspace = analyzerResult.ProjectResult;

            CodeBasedCheck(projectWorkspace, bindingsTransportMap);

            return bindingsTransportMap;
        }

        /// <summary>
        /// Check for Binding and Mode in Config based WCF Service
        /// </summary>
        /// <param name="projectDir">Project Directory for Config based WCF Service</param>
        /// <param name="bindingsTransportMap">Dictionary of binding and transport mode</param>
        public static void ConfigBasedCheck(string projectDir, Dictionary<string, BindingConfiguration> bindingsTransportMap)
        {
            var webConfig = WebConfigManager.LoadWebConfigAsXDocument(projectDir);
            var appConfig = WebConfigManager.LoadAppConfigAsXDocument(projectDir);

            if (webConfig.ContainsElement(Constants.WCFBindingElementPath))
            {
                BindingTagCheck(webConfig, bindingsTransportMap);
            }
            else if (appConfig.ContainsElement(Constants.WCFBindingElementPath))
            {
                BindingTagCheck(appConfig, bindingsTransportMap);
            }
            else if (webConfig.ContainsElement(Constants.WCFEndpointElementPath))
            {
                EndpointTagCheck(webConfig, bindingsTransportMap);
            }
            else if (appConfig.ContainsElement(Constants.WCFEndpointElementPath))
            {
                EndpointTagCheck(appConfig, bindingsTransportMap);
            }
        }

        /// <summary>
        /// Given XML Config with <bindings> element, check for binding and transport mode.
        /// </summary>
        /// <param name="config">XML object for config</param>
        /// <param name="bindingsTransportMap">Dictionary of binding and transport mode</param>
        public static void BindingTagCheck(WebConfigXDocument config, Dictionary<string, BindingConfiguration> bindingsTransportMap)
        {
            var bindingsElement = config.GetElementByPath(Constants.WCFBindingElementPath);

            var bindingsList = bindingsElement.Elements();

            foreach (var binding in bindingsList)
            {
                var bindingName = binding.Name.ToString().ToLower();

                var bindingElements = binding.Elements();
                foreach (var bindingElement in bindingElements)
                {
                    var modeList = bindingElement.Descendants(Constants.SecurityElement);

                    if (modeList.IsNullOrEmpty())
                    {
                        bindingsTransportMap[bindingName] = new BindingConfiguration();
                    }

                    foreach (var securityElement in modeList)
                    {
                        var modeName = securityElement.Attribute(Constants.ModeAttribute);

                        if (modeName != null)
                        {
                            bindingsTransportMap[bindingName] = new BindingConfiguration
                            {
                                Mode = modeName.Value.ToLower()
                            };
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Given XML Config with <endpoint> element check for binding.
        /// </summary>
        /// <param name="config">XML object for config</param>
        /// <param name="bindingsTransportMap">Dictionary of binding and transport mode</param>
        public static void EndpointTagCheck(WebConfigXDocument config, Dictionary<string, BindingConfiguration> bindingsTransportMap)
        {
            var endpointElement = config.GetElementByPath(Constants.WCFEndpointElementPath);

            var binding = endpointElement.Attribute(Constants.BindingAttribute);

            if (binding != null)
            {
                bindingsTransportMap[binding.Value.ToLower()] = new BindingConfiguration();
            }
        }

        /// <summary>
        /// Given XML Config with <protocolMapping> element, check for binding and transport mode.
        /// </summary>
        /// <param name="config">XML object for config</param>
        /// <param name="bindingsTransportMap">Dictionary of binding and transport mode</param>
        public static void ProtocolTagCheck(WebConfigXDocument config, Dictionary<string, BindingConfiguration> bindingsTransportMap)
        {
            var protocolElement = config.GetElementByPath(Constants.WCFProtocolMappingElement);

            var addProtocolElementsList = protocolElement.Elements(Constants.AddElement);
            foreach (var addProtocolElement in addProtocolElementsList)
            {
                var binding = addProtocolElement.Attribute(Constants.BindingAttribute);

                if (binding != null)
                {
                    var bindingName = binding.Value.ToLower();
                    bindingsTransportMap[bindingName] = new BindingConfiguration();
                }
            }
        }

        /// <summary>
        /// Check for Binding and Mode in Code based WCF Service
        /// </summary>
        /// <param name="project">ProjectWorkspace object of Code based WCF Service</param>
        /// <param name="bindingsTransportMap">Dictionary of binding and transport mode</param>
        public static void CodeBasedCheck(ProjectWorkspace project, Dictionary<string, BindingConfiguration> bindingsTransportMap)
        {
            IEnumerable<InvocationExpression> addEndpointInvocations = project.GetInvocationExpressionsByMethodName(Constants.AddServiceEndpointType);

            foreach (var addEndpointInvocation in addEndpointInvocations)
            {
                var argumentCount = addEndpointInvocation.Arguments.Count();

                if (argumentCount == 1)
                {
                    var endpointIdentifier = addEndpointInvocation.Arguments.First();

                    IEnumerable<ObjectCreationExpression> serviceEndpointObjectExpressions = project.GetObjectCreationExpressionBySemanticClassType(Constants.ServiceEndpointClass);

                    var endpointArgumentObjects = serviceEndpointObjectExpressions.
                        SelectMany(s => s.GetObjectCreationExpressionBySemanticNamespace(Constants.SystemServiceModelClass));

                    var bindingArgumentObjects = endpointArgumentObjects.Where(e => e.SemanticClassType != Constants.EndpointAddressType);

                    foreach(var binding in bindingArgumentObjects)
                    {
                        var bindingName = binding.SemanticClassType;
                        var bindingNameFormatted = bindingName.ToString().ToLower();

                        bindingsTransportMap[bindingName] = new BindingConfiguration
                        {
                            Mode = GetModeFromObjectArguments(binding.Arguments),
                            EndpointAddress = GetEndpointFromInvocationArguments(addEndpointInvocation.Arguments)
                        };
                    }
                }

                var bindingArgument = addEndpointInvocation.Arguments.Where(a => a.SemanticType.ToLower().Contains("binding"));

                var objectDeclarations = addEndpointInvocation.GetObjectCreationExpressionBySemanticNamespace(Constants.SystemServiceModelClass);

                ObjectCreationExpression objectCreationExpression;

                if(objectDeclarations.IsNullOrEmpty())
                {
                    if(!bindingArgument.IsNullOrEmpty())
                    {
                        var bindingName = bindingArgument.First().SemanticType;

                        var objectCreationExpressionList = project.GetObjectCreationExpressionBySemanticClassType(bindingName);

                        if(!objectCreationExpressionList.IsNullOrEmpty())
                        {
                            objectCreationExpression = objectCreationExpressionList.First();
                        }
                        else
                        {
                            bindingsTransportMap[bindingName] = new BindingConfiguration();
                            return;
                        }
                    }
                    else
                    { 
                        break; 
                    }
                }
                else
                {
                    objectCreationExpression = objectDeclarations.First();
                }

                if (objectCreationExpression != null)
                {
                    var bindingName = objectCreationExpression.SemanticClassType.ToLower();

                    bindingsTransportMap[bindingName] = new BindingConfiguration
                    {
                        Mode = GetModeFromObjectArguments(objectCreationExpression.Arguments),
                        EndpointAddress = GetEndpointFromInvocationArguments(addEndpointInvocation.Arguments)
                    };
                }
            }

            var webConfig = WebConfigManager.LoadWebConfigAsXDocument(project.ProjectRootPath);
            var appConfig = WebConfigManager.LoadAppConfigAsXDocument(project.ProjectRootPath);

            if (webConfig.ContainsElement(Constants.WCFBindingElementPath))
            {
                BindingTagCheck(webConfig, bindingsTransportMap);
            }
            else if (appConfig.ContainsElement(Constants.WCFBindingElementPath))
            {
                BindingTagCheck(appConfig, bindingsTransportMap);
            }

            if (webConfig.ContainsElement(Constants.WCFProtocolMappingElement))
            {
                ProtocolTagCheck(webConfig, bindingsTransportMap);
            }
            else if (appConfig.ContainsElement(Constants.WCFProtocolMappingElement))
            {
                ProtocolTagCheck(appConfig, bindingsTransportMap);
            }
        }

        /// <summary>
        /// Get Security Mode from Binding Object Creation Arguments.
        /// For instance : new BasicHttpBinding(BasicHttpSecurityMode.Message)
        /// </summary>
        /// <param name="arguments">List of Arguments which are part of Binding object creati</param>
        /// <returns>The Security Mode being used</returns>
        public static string GetModeFromObjectArguments(List<Argument> arguments)
        {
            var mode = Constants.NoneMode;

            foreach (var argument in arguments)
            {
                if (argument.SemanticType == Constants.BasicHttpSecurityMode || 
                    argument.SemanticType == Constants.BasicHttpsSecurityMode || 
                    argument.SemanticType == Constants.SecurityMode)
                {
                    mode = argument.Identifier.Substring(argument.Identifier.LastIndexOf(Constants.ModeSeparator) + 1);
                    break;
                }
            }

            return mode;
        }

        /// <summary>
        /// Get the endpoint string from Endpoint Invoction Arguments
        /// For instance : sh.AddServiceEndpoint(typeof(ISample), new BasicHttpBinding(BasicHttpSecurityMode.Transport), "/basicHttps")
        /// Get /basicHttps string
        /// </summary>
        /// <param name="arguments">Endpoint Invocation Arguments</param>
        /// <returns>Endpoint String</returns>
        public static string GetEndpointFromInvocationArguments(List<Argument> arguments)
        {
            string endpointAddress = null;

            foreach (var argument in arguments)
            {
                if (argument.SemanticType != null && argument.SemanticType.ToLower() == Constants.StringType)
                {
                    endpointAddress = argument.Identifier;
                    break;
                }
            }

            return endpointAddress;
        }

        /// <summary>
        /// For Code Based, check for Service Interface and class which implements the same
        /// </summary>
        /// <param name="project">ProjectWorkspace object for Code Based Service</param>
        /// <returns>A tuple of Service Interface and Implementing Class if any, otherwise null</returns>
        public static Tuple<string, string> GetServiceInterfaceAndClass(ProjectWorkspace project)
        {
            Tuple<string, string> serviceInterfaceAndClass;

            var interfaces = project.GetAllInterfaceDeclarations()?.ToList();
            if (interfaces.IsNullOrEmpty()) { return null; }

            var interfacesWithServiceContract = interfaces
                .Where(i => i.HasAttribute(Constants.ServiceContractAttribute))
                ?.ToList();
            if (interfacesWithServiceContract.IsNullOrEmpty()) { return null; }

            var interfaceWithServiceContractMethods = interfacesWithServiceContract
                .SelectMany(i => i.GetMethodDeclarations())?.ToList();
            if (interfaceWithServiceContractMethods.IsNullOrEmpty()) { return null; }

            var serviceInterfaceMethodWithObjectContract = interfaceWithServiceContractMethods
                .Where(m => m.HasAttribute(Constants.OperationContractAttribute))
                ?.ToList();

            if (!serviceInterfaceMethodWithObjectContract.IsNullOrEmpty())
            {
                var classes = project.GetAllClassDeclarations()?.ToList();
                if (classes.IsNullOrEmpty()) { return null; }

                foreach (var interfaceWithServiceContract in interfacesWithServiceContract)
                {
                    foreach (var classDeclaration in classes)
                    {
                        if (classDeclaration.InheritsInterface(interfaceWithServiceContract.Identifier))
                        {
                            //Filter out generated Code
                            var IsGeneratedCode = classDeclaration.HasAnnotation(Constants.DebuggerStepThroughAttribute)
                                || classDeclaration.HasAnnotation(Constants.GeneratedCodeAttribute);

                            if(!IsGeneratedCode)
                            {
                                serviceInterfaceAndClass = new Tuple<string, string>(interfaceWithServiceContract.Identifier, 
                                    classDeclaration.Identifier);

                                return serviceInterfaceAndClass;
                            }
                        }
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }
    }
}
