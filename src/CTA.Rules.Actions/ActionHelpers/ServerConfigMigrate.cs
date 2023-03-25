using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using CTA.Rules.Actions.ActionHelpers;
using CTA.Rules.Config;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CTA.Rules.Actions
{
    public class ServerConfigMigrate
    {
        private readonly string _projectDir;
        private readonly ProjectType _projectType;
        private List<string> _requiredDirectives;
        private Dictionary<string, List<string>> _middleWareExpressions;
        private List<string> _kestrelOptions;
        private List<string> _addNugetReference;
        private string _startupFilePath;
        private string _programcsFilePath;
        SyntaxNode _startupRoot;
        SyntaxNode _programcsRoot;

        public ServerConfigMigrate(string projectDir, ProjectType projectType)
        {
            _projectDir = projectDir;
            _projectType = projectType;
            Init();
        }
        private void Init()
        {
            _requiredDirectives = new List<string>();
            _middleWareExpressions = new Dictionary<string, List<string>>();
            _kestrelOptions = new List<string>();
            _addNugetReference = new List<string>();
            foreach (var methodName in ServerConfigTemplates.MiddlewareConfigMethods)
            {
                _middleWareExpressions.Add(methodName, new List<string>());
            }

            _startupFilePath = Path.Combine(_projectDir, FileTypeCreation.Startup.ToString() + ".cs");
            _programcsFilePath = Path.Combine(_projectDir, FileTypeCreation.Program.ToString() + ".cs");

            _startupRoot = CSharpSyntaxTree.ParseText(File.ReadAllText(_startupFilePath)).GetRoot();
            _programcsRoot = CSharpSyntaxTree.ParseText(File.ReadAllText(_programcsFilePath)).GetRoot();
        }

        public void PortServerConfiguration(ConfigurationSection serverConfig)
        {
            try
            {
                //Middleware pipeline should be in a specific order
                //Add default pre-middleware expressions
                AddPreMiddlewareExpessions();

                XDocument xDocument = XDocument.Parse(serverConfig.SectionInformation.GetRawXml());
                var element = xDocument.Elements().ToList();


                foreach (string attribute in ServerConfigTemplates.ConfigAttributes)
                {
                    var childAttributes = element.FirstOrDefault()?.Descendants().Where(x => x.Name.ToString().Equals(attribute, StringComparison.OrdinalIgnoreCase));
                    if (childAttributes.HasAny())
                    {
                        try
                        {
                            LogHelper.LogInformation($"Migrate {attribute} IIS server config to kestrel");
                            InvokeMethod(attribute, new object[] { childAttributes.First() });
                        }
                        catch(Exception ex)
                        {
                            LogHelper.LogError(ex, $"Error porting web server attribute: {attribute}");
                        }
                    }           
                }

                //Add default post-middleware expressions
                AddPostMiddlewareExpessions();

                RegisterMiddlewareComponents();

                File.WriteAllText(_startupFilePath, _startupRoot.ToFullString());
                File.WriteAllText(_programcsFilePath, _programcsRoot.ToFullString());
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error porting web server configuration");
            }
        }

        //Handle security attributes
        private void HandleSecurity(XElement securityAttribute)
        {

            HandleAuthorization();
            foreach (var attribute in securityAttribute.Elements())
            {
                //Handle authentication
                if (attribute.Name.ToString().Equals(WebServerConfigAttributes.Authentication.ToString(), System.StringComparison.OrdinalIgnoreCase))
                {
                    HandleAuthentication(attribute);
                }

                // Handle request filtering
                if (attribute.Name.ToString().Equals(WebServerConfigAttributes.RequestFiltering.ToString(), System.StringComparison.OrdinalIgnoreCase))
                {
                    HandleRequestFiltering(attribute);
                }
            }
        }

        // Handle web server attributes
        private void HandleRequestFiltering(XElement requestFilteringElement)
        {
            foreach (var attribute in requestFilteringElement.Elements())
            {
                // Request limits
                if (attribute.Name.ToString().Equals(WebServerConfigAttributes.RequestLimits.ToString(), System.StringComparison.OrdinalIgnoreCase))
                {
                    var maxAllowedContentLength = GetAttributeValue(attribute, Constants.MaxAllowedContentLength);
                    if (maxAllowedContentLength != null)
                    {
                        _kestrelOptions.Add(string.Format(ServerConfigTemplates.KestrelOptionsTemplates[WebServerConfigAttributes.RequestLimits.ToString()], Int64.Parse(maxAllowedContentLength)));
                    }
                }
            }
        }

        //Handle Http redirect
        private void HandleHttpRedirect(XElement httpRedirectElement)
        {
            StringBuilder sb = new StringBuilder("new RewriteOptions()");
            bool isRedirectionEnabled = bool.Parse(GetAttributeValue(httpRedirectElement, Constants.Enabled));
            if (!isRedirectionEnabled)
                return;

            string responeStatus = GetAttributeValue(httpRedirectElement, Constants.HttpResponseStatus);
            //default response code
            int responseStatusCode = responeStatus != null ? ServerConfigTemplates.HttpResponseStatus[responeStatus] : 302;

            foreach (var element in httpRedirectElement.Elements())
            {
                if (element.Name.ToString().Trim().Equals(Constants.Add))
                {
                    var wildcard = GetAttributeValue(element, Constants.WildCard);
                    var destinationUrl = GetAttributeValue(element, Constants.Destination);

                    sb.Append(string.Format(ServerConfigTemplates.addRedirectTemplate, wildcard, destinationUrl, responseStatusCode));
                }
            }

            _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(string.Format(ServerConfigTemplates.ConfigurationExpressionTemplates[WebServerConfigAttributes.HttpRedirect.ToString()], sb.ToString()));
            _requiredDirectives.AddRange(ServerConfigTemplates.Directives[WebServerConfigAttributes.HttpRedirect.ToString()]);
        }

        //Handle Http modules
        private void HandleModules(XElement httpModuleElement)
        {
            foreach (var module in httpModuleElement.Elements())
            {
                // only inject added modules to middleware
                if (module.Name.ToString().Trim().Equals(Constants.Add))
                {
                    var moduleName = GetAttributeValue(module, Constants.Type);
                    _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(string.Format(ServerConfigTemplates.ConfigurationExpressionTemplates[WebServerConfigAttributes.Modules.ToString()], moduleName));
                }
            }
        }

        //Handle Http handlers
        private void HandleHandlers(XElement httpHandlerElement)
        {
            foreach (var handler in httpHandlerElement.Elements())
            {
                // only inject added handlers to middleware
                if (handler.Name.ToString().Trim().Equals(Constants.Add))
                {
                    var handlerName = GetAttributeValue(handler, Constants.Type);
                    var handlerPath = GetAttributeValue(handler, Constants.PathAttribute, "*");
                    if(!ServerConfigTemplates.UnsupportedHandlers.Contains(handlerName.Trim()))
                        _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(string.Format(ServerConfigTemplates.ConfigurationExpressionTemplates[WebServerConfigAttributes.Handlers.ToString()], handlerPath, handlerName));
                }
            }
        }

        //Handle Http compression
        private void HandleHttpCompression(XElement httpCompressionElement)
        {
            string httpCompression = WebServerConfigAttributes.HttpCompression.ToString();
            List<string> mimeTypeList = new List<string>();

            foreach(string compresionType in ServerConfigTemplates.CompresionTypes)
            {
                var mimeTypes = httpCompressionElement.Descendants(compresionType);
                if(mimeTypes.HasAny())
                {
                    foreach (var mimeType in mimeTypes.Elements())
                    {
                        // only inject added mimeTypes to middleware
                        if (mimeType.Name.ToString().Trim().Equals(Constants.Add))
                        {
                            var mimeTypeSupported = GetAttributeValue(mimeType, Constants.MimeType);
                            bool isMimeTypeEnabled = bool.Parse(GetAttributeValue(mimeType, Constants.Enabled));
                            if (isMimeTypeEnabled)
                            {
                                mimeTypeList.Add(mimeTypeSupported);
                            }
                        }
                    }
                }

            }
            string mimeTypecsv = "\"" + string.Join("\", \"", mimeTypeList) + "\"";
            _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(ServerConfigTemplates.ConfigurationExpressionTemplates[httpCompression]);
            _middleWareExpressions[ServerConfigTemplates.ConfigureServicesMethod].Add(ServerConfigTemplates.ServiceExpressionTemplates[httpCompression].Replace("mime_types", mimeTypecsv));
            _requiredDirectives.AddRange(ServerConfigTemplates.Directives[httpCompression]);

        }

        private void HandleAuthorization()
        {
            //Handle authorization
            string authorization = WebServerConfigAttributes.Authorization.ToString();

            //_middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(ServerConfigTemplates.ConfigurationExpressionTemplates[authorization]);
            _middleWareExpressions[ServerConfigTemplates.ConfigureServicesMethod].Add(ServerConfigTemplates.ServiceExpressionTemplates[authorization]);
        }

        private void HandleAuthentication(XElement authenticationAttributeElement)
        {
            _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(ServerConfigTemplates.ConfigurationExpressionTemplates[WebServerConfigAttributes.Authentication.ToString()]);
            foreach (var authenticationType in authenticationAttributeElement.Elements())
            {
                // window authentication
                if (authenticationType.Name.ToString().Equals(WebServerConfigAttributes.WindowsAuthentication.ToString(), System.StringComparison.OrdinalIgnoreCase))
                {
                    bool isAuthEnabled = bool.Parse(GetAttributeValue(authenticationType, Constants.Enabled, ""));
                    if (isAuthEnabled)
                    {
                        string windowsAuthentication = WebServerConfigAttributes.WindowsAuthentication.ToString();

                        _middleWareExpressions[ServerConfigTemplates.ConfigureServicesMethod].Add(ServerConfigTemplates.ServiceExpressionTemplates[windowsAuthentication]);
                        _requiredDirectives.AddRange(ServerConfigTemplates.Directives[windowsAuthentication]);
                        _addNugetReference.AddRange(ServerConfigTemplates.Directives[windowsAuthentication]);
                    }
                }
            }
        }


        private void RegisterMiddlewareComponents()
        {
            foreach (var kvp in _middleWareExpressions)
            {
                if (kvp.Value.Count() == 0)
                    continue;

                var node = _startupRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(method => method.Identifier.Text == kvp.Key);
                var newNode = node;
                List<StatementSyntax> expressions = new List<StatementSyntax>();

                foreach (var expression in kvp.Value)
                {
                    StatementSyntax parsedExpression = SyntaxFactory.ParseStatement(expression);

                    if (!parsedExpression.FullSpan.IsEmpty)
                    {
                        expressions.Add(parsedExpression);
                    }
                }
                BlockSyntax nodeBody = SyntaxFactory.Block(expressions.ToArray());
                newNode = node.WithBody(nodeBody).NormalizeWhitespace();
                _startupRoot = _startupRoot.ReplaceNode(node, newNode);
            }

            //Add Kestrel options
            if (_kestrelOptions.Count > 0)
                AddKestrelOptions();

            //Add directives
            AppendDirectives();

            AddComments();
        }

        private void AppendDirectives()
        {
            var node = _startupRoot as CompilationUnitSyntax;
            var allUsings = node.Usings;
            if (node != null)
            {
                foreach (var directive in _requiredDirectives)
                {
                    var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(directive)).NormalizeWhitespace();
                    allUsings = allUsings.Add(usingDirective);
                }
                node = node.WithUsings(allUsings).NormalizeWhitespace();
            }
            _startupRoot = node;
        }

        private void AddPreMiddlewareExpessions()
        {
            if(_projectType == ProjectType.Mvc)
            {
                _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].AddRange(ServerConfigTemplates.DefaultPreMiddleWareTemplates[ProjectType.Mvc]);
                _middleWareExpressions[ServerConfigTemplates.ConfigureServicesMethod].AddRange(ServerConfigTemplates.DefaultServiceExpressionTemplates[ProjectType.Mvc]);
            }
            else if(_projectType == ProjectType.WebApi)
            {
                _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].AddRange(ServerConfigTemplates.DefaultPreMiddleWareTemplates[ProjectType.WebApi]);
                _middleWareExpressions[ServerConfigTemplates.ConfigureServicesMethod].AddRange(ServerConfigTemplates.DefaultServiceExpressionTemplates[ProjectType.WebApi]);
            }
        }

        private void AddPostMiddlewareExpessions()
        {
            if (_projectType == ProjectType.Mvc)
            {
                _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].AddRange(ServerConfigTemplates.DefaultPostMiddleWareTemplates[ProjectType.Mvc]);
            }
            else if (_projectType == ProjectType.WebApi)
            {
                _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].AddRange(ServerConfigTemplates.DefaultPostMiddleWareTemplates[ProjectType.WebApi]);
            }
        }

        private void AddKestrelOptions()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var option in _kestrelOptions)
            {
                sb.Append(option);
            }

            var expression = ServerConfigTemplates.kestrelTemplate.Replace("kestrel_options", sb.ToString());
            StatementSyntax parsedExpression = SyntaxFactory.ParseStatement(expression);

            var node = _programcsRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(x => x.Identifier.ToString().Trim().Equals(ServerConfigTemplates.ConfigureHostBuilderMethod));
            var lambdaNode = node.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().FirstOrDefault(x => x.Parameter.ToString().Trim().Equals(ServerConfigTemplates.LambdaWebBuilderAttribute));

            var newLambdaNode = lambdaNode.AddBlockStatements(new StatementSyntax[] { parsedExpression });

            _programcsRoot = _programcsRoot.ReplaceNode(lambdaNode, newLambdaNode);
        }

        private void AddComments()
        {
            StringBuilder sb = new StringBuilder("Please add the correponding references.");

            if(_addNugetReference.Count > 0)
            {
                sb.Append("Add the following nuget package references ");
                sb.Append(string.Join(",", _addNugetReference));
            }
            sb.Append(".");
            sb.Append(string.Join(".", ServerConfigTemplates.AdditonalComments));

            _startupRoot = CommentHelper.AddCSharpComment(_startupRoot, sb.ToString());
        }

        private string GetAttributeValue(XElement element, string attributeName, string splitChar = null)
        {
            if (string.IsNullOrEmpty(splitChar))
            {
                return element.Attribute(attributeName)?.Value;
            }
            return element.Attribute(attributeName)?.Value.Split(splitChar).Last();
        }

        private void InvokeMethod(string methodName, object[] args)
        {
            Type type = this.GetType();
            string methodNameToInvoke = "Handle" + methodName;

            if (string.IsNullOrWhiteSpace(methodName))
                return;

            MethodInfo method = type.GetMethod(methodNameToInvoke, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                LogHelper.LogInformation("Cannot find corresponding method in ServerConfigMigrate" + methodNameToInvoke);
                return;
            }

            method.Invoke(this, args);
        }
    }
}
