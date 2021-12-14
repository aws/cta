using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.WCF;
using CTA.FeatureDetection.Common.WCFConfigUtils;
using CTA.Rules.Common.WebConfigManagement;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Constants = CTA.Rules.PortCore.WCF.Constants;

namespace CTA.Rules.PortCore
{
    public class WCFServicePort
    {
        private string _projectPath;
        private ProjectType _projectType;
        private AnalyzerResult _analyzerResult;

        public WCFServicePort(string projectPath, ProjectType projectType, AnalyzerResult analyzerResult)
        {
            _projectPath = projectPath;
            _projectType = projectType;
            _analyzerResult = analyzerResult;
        }

        /// <summary>
        /// Generate New Config File for CoreWCF.
        /// </summary>
        /// <returns>New Config File for CoreWCF</returns>
        public string GetNewConfigFile()
        {
            var webConfig = WebConfigManager.LoadWebConfigAsXDocument(_projectPath);
            var appConfig = WebConfigManager.LoadAppConfigAsXDocument(_projectPath);

            WebConfigXDocument config;

            if (webConfig.ContainsElement(Constants.SystemServiceModelElementPath))
            {
                config = webConfig;
            }
            else if (appConfig.ContainsElement(Constants.SystemServiceModelElementPath))
            {
                config = appConfig;
            }
            else
            {
                return null;
            }

            var wcfConfig = config.GetDescendantsAndSelf(Constants.SystemServiceModelElement);

            if (!wcfConfig.IsNullOrEmpty())
            {
                var newXDoc = new XDocument(new XDeclaration(Constants.ConfigXMLVersion, Constants.ConfigXMLEncoding, Constants.ConfigXMLStandalone), wcfConfig.First());

                newXDoc.Descendants().Where(d => d.Name == Constants.HostElement).Remove();
                newXDoc.Descendants().Where(d => d.Name == Constants.EndpointElement && d.Attribute(Constants.BindingAttribute)?.Value == Constants.MexBinding).Remove();

                var serviceModelElement = newXDoc.Element(Constants.SystemServiceModelElement);

                serviceModelElement.ReplaceWith(new XElement(Constants.ConfigurationElement, serviceModelElement));

                var newConfigFile = new StringWriter();
                newXDoc.Save(newConfigFile);

                return newConfigFile.ToString();
            }

            return config.GetDocAsString();

        }

        /// <summary>
        /// Get Config File Path for the Project.
        /// </summary>
        /// <returns>Config File Path</returns>
        public string GetConfigFilePath()
        {
            var webConfig = WebConfigManager.LoadWebConfigAsXDocument(_projectPath);
            var appConfig = WebConfigManager.LoadAppConfigAsXDocument(_projectPath);

            if (webConfig.ContainsElement(Constants.SystemServiceModelElementPath))
            {
                return Path.Combine(_projectPath, Constants.WebConfig);
            }
            else if (appConfig.ContainsElement(Constants.SystemServiceModelElementPath))
            {
                return Path.Combine(_projectPath, Constants.AppConfig);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Given existing template Program.cs Syntax Tree, Add configurations and generate new Syntax Tree.
        /// </summary>
        /// <param name="programTree">Syntax Tree for existing Program.cs Template</param>
        /// <returns>New root with updated Program.cs</returns>
        public SyntaxNode ReplaceProgramFile(SyntaxTree programTree)
        {
            Dictionary<string, int> transportPort = new Dictionary<string, int>();
            Dictionary<string, BindingConfiguration> bindingModeMap = new Dictionary<string, BindingConfiguration>();

            if (_projectType == ProjectType.WCFConfigBasedService)
            {
                string projectDir = _analyzerResult.ProjectResult.ProjectRootPath;
                bindingModeMap = GetBindingsTransportMap(projectDir);
                AddBinding(bindingModeMap, transportPort);
            }
            else
            {
                ProjectWorkspace project = _analyzerResult.ProjectResult;
                bindingModeMap = GetBindingsTransportMap(project);
                AddBinding(bindingModeMap, transportPort);
            }

            if(transportPort.IsNullOrEmpty())
            {
                return programTree.GetRoot();
            }

            var containsTransportRelatedMode = 
                bindingModeMap.Any(b => 
                b.Value.Mode.ToLower() == Constants.TransportMessageCredentialsMode.ToLower() || 
                b.Value.Mode.ToLower() == Constants.TransportMode.ToLower());

            var newRoot = ReplaceProgramNode(transportPort, programTree, containsTransportRelatedMode);

            return newRoot;
        }

        /// <summary>
        /// Generate Program.cs by updating Program.cs template Syntax tree.
        /// </summary>
        /// <param name="transportPort">Map of Binding And Port</param>
        /// <param name="programTree">Existing Program.cs Template SyntaxTree</param>
        /// <param name="containsTransportRelatedMode">Flag to check if any binding uses TransportWithMessage Mode</param>
        /// <returns>New root with updated Program.cs contents</returns>
        public static SyntaxNode ReplaceProgramNode(Dictionary<string, int> transportPort, SyntaxTree programTree, bool containsTransportRelatedMode)
        {
            string httpListen = Constants.ListenLocalHostFormat;
            string httpsListen = Constants.ListenHttpsFormat;
            string netTcpMethodExpression = Constants.NetTcpFormat;

            var root = programTree.GetRoot();

            var lambdaExpressionList = root.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>();
            if (lambdaExpressionList.IsNullOrEmpty())
            {
                return root;
            }
            var lambdaExpression = lambdaExpressionList.First();

            var block = lambdaExpression.Block;
            var newBlock = block;

            var parameter = lambdaExpression.Parameter;

            if (transportPort.ContainsKey(Constants.HttpProtocol))
            {
                httpListen = String.Format(httpListen, parameter.Identifier.ValueText, transportPort.GetValueOrDefault(Constants.HttpProtocol, 8080));
                newBlock = block.AddStatements(SyntaxFactory.ParseStatement(httpListen));
            }

            if (transportPort.ContainsKey(Constants.HttpsProtocol) || containsTransportRelatedMode)
            {
                httpsListen = String.Format(httpsListen, parameter.Identifier.ValueText, transportPort.GetValueOrDefault(Constants.HttpsProtocol, 8888));
                newBlock = newBlock.AddStatements(SyntaxFactory.ParseStatement(httpsListen));
            }

            var newLambdaExpression = lambdaExpression.ReplaceNode(block, newBlock);

            root = root.ReplaceNode(lambdaExpression, newLambdaExpression);

            var memberAccessExpressions = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>().ToList();
            MemberAccessExpressionSyntax kestrelInvocationNode = null;
            foreach (MemberAccessExpressionSyntax memberAccessExpression in memberAccessExpressions)
            {
                if (memberAccessExpression.Name.Identifier.Text.Equals(Constants.UseStartupMethodIdentifier))
                {
                    kestrelInvocationNode = memberAccessExpression;
                    break;
                }
            }

            if (transportPort.ContainsKey(Constants.NettcpProtocol))
            {
                var netTCPExpression = SyntaxFactory
                            .MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            kestrelInvocationNode.Expression,
                            SyntaxFactory.IdentifierName(netTcpMethodExpression)
                            );
                var netTcpInvocation = SyntaxFactory.InvocationExpression(netTCPExpression, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(transportPort.GetValueOrDefault(Constants.NettcpProtocol, 8000))))
               })));
                var kestrelInvocationWithNetTcp = kestrelInvocationNode.WithExpression(netTcpInvocation);
                root = root.ReplaceNode(kestrelInvocationNode, kestrelInvocationWithNetTcp);
            }

            return root;
        }

        /// <summary>
        /// Given existing Startup.cs template file, generate new Startup.cs based on configuration.
        /// </summary>
        /// <param name="startupFilePath">Path to existing Startup.cs template</param>
        /// <returns>New Startup.cs contents</returns>
        public string ReplaceStartupFile(string startupFilePath)
        {
            var startupFileContents = File.ReadAllText(startupFilePath);

            if (_projectType == ProjectType.WCFConfigBasedService)
            {
                string configPath = Path.Combine(_projectPath, Constants.PortedConfigFileName);

                if (HasBehavioursTag())
                {
                    startupFileContents = HandleBehaviorsTag(startupFilePath);
                }

                return startupFileContents.Replace(Constants.XMLPathPlaceholder, "@\"" + configPath + "\"");
            }
            else
            {
                string addService = Constants.AddServiceFormat;
                string endpointConfigTemplate = Constants.AddServiceEndpointFormat;
                string endpointConfigs = "";

                ProjectWorkspace project = _analyzerResult.ProjectResult;

                Dictionary<string, BindingConfiguration> bindingTransportMap = GetBindingsTransportMap(project);

                Tuple<string, string> serviceInterfaceAndClass = WCFBindingAndTransportUtil.GetServiceInterfaceAndClass(project);

                var serviceInterfaceName = serviceInterfaceAndClass.Item1 ?? Constants.DefaultServiceInterface;
                var serviceClassName = serviceInterfaceAndClass.Item2 ?? Constants.DefaultServiceClass;

                endpointConfigs += String.Format(addService, serviceClassName);

                foreach(KeyValuePair<string, BindingConfiguration> keyValuePair in bindingTransportMap)
                {
                    var binding = keyValuePair.Key;
                    var mode = keyValuePair.Value.Mode;
                    var endpointAddress = keyValuePair.Value.EndpointAddress ?? String.Join("", "\"", "/", binding.ToLower(), "\"");

                    if (mode.ToLower() == Constants.TransportMode.ToLower())
                    {
                        mode = Constants.TransportMode;
                    }
                    else if (mode.ToLower() == Constants.TransportMessageCredentialsMode.ToLower())
                    {
                        mode = Constants.TransportMessageCredentialsMode;
                    }

                    if (binding == Constants.HttpProtocol)
                    {
                        endpointConfigs += String.Format(endpointConfigTemplate, serviceClassName, serviceInterfaceName, 
                            mode == Constants.NoneMode ? "new BasicHttpBinding()" : "new BasicHttpBinding(BasicHttpSecurityMode." + mode + ")", endpointAddress);
                    }
                    else if(binding == Constants.NettcpProtocol)
                    {
                        endpointConfigs += String.Format(endpointConfigTemplate, serviceClassName, serviceInterfaceName,
                            mode == Constants.NoneMode ? "new NetTcpBinding()" : "new NetTcpBinding(SecurityMode." + mode + ")", endpointAddress);
                    }
                    else if (binding == Constants.WSHttpProtocol)
                    {
                        endpointConfigs += String.Format(endpointConfigTemplate, serviceClassName, serviceInterfaceName,
                            mode == Constants.NoneMode ? "new WSHttpBinding()" : "new WSHttpBinding(SecurityMode." + mode + ")", endpointAddress);
                    }
                    else if (binding == Constants.HttpsProtocol)
                    {
                        endpointConfigs += String.Format(endpointConfigTemplate, serviceClassName, serviceInterfaceName, "new BasicHttpBinding(BasicHttpSecurityMode.Transport)", endpointAddress);
                    }
                    else if (binding == Constants.NethttpProtocol)
                    {
                        endpointConfigs += String.Format(endpointConfigTemplate, serviceClassName, serviceInterfaceName,
                            mode == Constants.NoneMode ? "new NetHttpBinding()" : "new NetHttpBinding(BasicHttpSecurityMode." + mode + ")", endpointAddress);
                    }
                }

                return startupFileContents.Replace(Constants.EndpointPlaceholder, endpointConfigs);
            }
        }


        /// <summary>
        /// Determines whether project config has <behaviors> tag
        /// </summary>
        /// <returns>Whether <behaviors> tag is present</returns>
        public bool HasBehavioursTag()
        {
            var config = WebConfigManager.LoadWebConfigAsXDocument(_projectPath);

            return config.ContainsElement(Constants.BehaviorsPath);
        }

        /// <summary>
        /// For Behaviors Tag, add Comment specifying it is unsupported in CoreWCF.s
        /// </summary>
        /// <param name="filePath">Path to existing Startup.cs</param>
        /// <returns>New Startup.cs contents with Comment</returns>
        public static string HandleBehaviorsTag(string filePath)
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var newFileContents = new StringBuilder();

            foreach (var line in lines)
            {
                newFileContents.AppendLine(line);

                if (line.Contains(Constants.WCFConfigManagerAPI))
                {
                    newFileContents
                        .Append(Constants.WCFBehaviorsMessage);
                }
            }
            return newFileContents.ToString();
        }

        public static Dictionary<string, BindingConfiguration> GetBindingsTransportMap(ProjectWorkspace project)
        {
            Dictionary<string, BindingConfiguration> bindingsTransportMap = new Dictionary<string, BindingConfiguration>();

            WCFBindingAndTransportUtil.CodeBasedCheck(project, bindingsTransportMap);

            return bindingsTransportMap;
        }

        public static Dictionary<string, BindingConfiguration> GetBindingsTransportMap(string projectDir)
        {
            Dictionary<string, BindingConfiguration> bindingsTransportMap = new Dictionary<string, BindingConfiguration>();

            WCFBindingAndTransportUtil.ConfigBasedCheck(projectDir, bindingsTransportMap);

            return bindingsTransportMap;
        }

        public static void AddBinding(Dictionary<string, BindingConfiguration> bindingsTransportMap, Dictionary<string, int> transportPortMap)
        {
            foreach (var binding in bindingsTransportMap.Keys)
            {
                if (binding == Constants.HttpProtocol || binding == Constants.NethttpProtocol || binding == Constants.WSHttpProtocol)
                {
                    transportPortMap.Add(Constants.HttpProtocol, Constants.HttpDefaultPort);
                }
                else if (binding == Constants.HttpsProtocol)
                {
                    transportPortMap.Add(Constants.HttpsProtocol, Constants.HttpsDefaultPort);
                }
                else if (binding == Constants.NettcpProtocol)
                {
                    transportPortMap.Add(Constants.NettcpProtocol, Constants.NetTcpDefaultPort);
                }
            }
        }
    }
}
