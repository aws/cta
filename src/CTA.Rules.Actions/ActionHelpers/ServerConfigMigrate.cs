using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
        private List<string> _requiredDirectives;
        private Dictionary<string, List<string>> _middleWareExpressions;
        private string _startupFilePath;
        SyntaxNode _root;

        public ServerConfigMigrate(string projectDir)
        {
            _projectDir = projectDir;
            Init();
        }
        private void Init()
        {
            _requiredDirectives = new List<string>();
            _middleWareExpressions = new Dictionary<string, List<string>>();
            foreach (var methodName in ServerConfigTemplates.MiddlewareConfigMethods)
            {
                _middleWareExpressions.Add(methodName, new List<string>());
            }

            _startupFilePath = Path.Combine(_projectDir, FileTypeCreation.Startup.ToString() + ".cs");
            var code = File.ReadAllText(_startupFilePath);
            _root = CSharpSyntaxTree.ParseText(code).GetRoot();

        }

        public void PortServerConfiguration(ConfigurationSection serverConfig)
        {
            try
            {
                XDocument xDocument = XDocument.Parse(serverConfig.SectionInformation.GetRawXml());
                var element = xDocument.Elements().ToList();

                //Handle Http modules
                var httpModules = element.FirstOrDefault()?.Descendants(WebServerConfigAttributes.Modules.ToString().ToLower());
                if (httpModules.HasAny())
                {
                    HandleHttpModules(httpModules);
                }

                //Handle Http handlers
                var httpHandlers = element.FirstOrDefault()?.Descendants(WebServerConfigAttributes.Handlers.ToString().ToLower());
                if (httpHandlers.HasAny())
                {
                    HandleHttpHandlers(httpHandlers);
                }

                // Handle security Attributes
                var securityAttribute = element.FirstOrDefault()?.Descendants(WebServerConfigAttributes.Security.ToString().ToLower());
                if (securityAttribute.HasAny())
                {
                    HandleAuthorization();

                    //Handle authentication
                    var authenticationAttribute = securityAttribute.FirstOrDefault().Descendants(WebServerConfigAttributes.Authentication.ToString().ToLower());
                    if (authenticationAttribute.HasAny())
                    {
                        HandleAuthentication(authenticationAttribute);
                    }

                }

                RegisterMiddlewareComponents();

                File.WriteAllText(_startupFilePath, _root.ToFullString());
            }
            catch(Exception ex)
            {
                LogHelper.LogError(ex, "Error porting web server configuration");
            }       
        }

        // Handle web server attributes
        private void HandleHttpModules(IEnumerable<XElement> httpModules)
        {
            foreach (var module in httpModules.Elements())
            {
                // only inject added modules to middleware
                if (module.Name.ToString().Trim().Equals("add"))
                {
                    var moduleName = GetAttributeValue(module, "type");
                    _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(string.Format(ServerConfigTemplates.ConfigurationExpressionTemplates[WebServerConfigAttributes.Modules.ToString()], moduleName));
                }
            }
        }

        private void HandleHttpHandlers(IEnumerable<XElement> httpHandlers)
        {
            foreach (var handler in httpHandlers.Elements())
            {
                // only inject added handlers to middleware
                if (handler.Name.ToString().Trim().Equals("add"))
                {
                    var handlerName = GetAttributeValue(handler, "type");
                    var handlerPath = GetAttributeValue(handler, "path", "*");
                    _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(string.Format(ServerConfigTemplates.ConfigurationExpressionTemplates[WebServerConfigAttributes.Handlers.ToString()], handlerPath, handlerName));
                }
            }
        }

        private void HandleAuthorization()
        {
            //Handle authorization
            string authorization = WebServerConfigAttributes.Authorization.ToString();

            _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(ServerConfigTemplates.ConfigurationExpressionTemplates[authorization]);
            _middleWareExpressions[ServerConfigTemplates.ConfigureServicesMethod].Add(ServerConfigTemplates.ServiceExpressionTemplates[authorization]);
        }

        private void HandleAuthentication(IEnumerable<XElement> authenticationAttribute)
        {
            _middleWareExpressions[ServerConfigTemplates.ConfigureMiddlewareMethod].Add(ServerConfigTemplates.ConfigurationExpressionTemplates[WebServerConfigAttributes.Authentication.ToString()]);
            foreach (var authenticationType in authenticationAttribute.Elements())
            {
                // window authentication
                if (authenticationType.Name.ToString().Equals(WebServerConfigAttributes.WindowsAuthentication.ToString(), System.StringComparison.OrdinalIgnoreCase))
                {
                    bool isAuthEnabled = bool.Parse(GetAttributeValue(authenticationType, "enabled", ""));
                    if (isAuthEnabled)
                    {
                        string windowsAuthentication = WebServerConfigAttributes.WindowsAuthentication.ToString();

                        _middleWareExpressions[ServerConfigTemplates.ConfigureServicesMethod].Add(ServerConfigTemplates.ServiceExpressionTemplates[windowsAuthentication]);
                        _requiredDirectives.AddRange(ServerConfigTemplates.Directives[windowsAuthentication]);
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

                var node = _root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(method => method.Identifier.Text == kvp.Key);
                var newNode = node;
                List<StatementSyntax> expressions = new List<StatementSyntax>();

                foreach (var expression in kvp.Value)
                {
                    //commenting out code to reduce build errors
                    var startComment = SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, "/*");
                    var endComment = SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, "*/");

                    StatementSyntax parsedExpression = SyntaxFactory.ParseStatement(expression).WithLeadingTrivia(new SyntaxTriviaList(startComment)).WithTrailingTrivia(new SyntaxTriviaList(endComment));
                    if (!parsedExpression.FullSpan.IsEmpty)
                    {
                        expressions.Add(parsedExpression);
                    }
                }
                newNode = node.AddBodyStatements(expressions.ToArray()).NormalizeWhitespace();
                _root = _root.ReplaceNode(node, newNode);
            }

            //Add directives
            AppendDirectives();
        }

        private void AppendDirectives()
        {
            var node = _root as CompilationUnitSyntax;
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
            _root = node;
        }

        private string GetAttributeValue(XElement element, string attributeName, string splitChar = null)
        {
            if (string.IsNullOrEmpty(splitChar))
            {
                return element.Attribute(attributeName)?.Value;
            }
            return element.Attribute(attributeName)?.Value.Split(splitChar).Last();
        }
    }
}
