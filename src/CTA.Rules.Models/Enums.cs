using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Namotion.Reflection;

namespace CTA.Rules.Models
{
    public enum ActionTypes
    {
        Method,
        Expression,
        Using,
        Class,
        Interface,
        Identifier,
        Attribute,
        AttributeList,
        Package,
        Namespace,
        ObjectCreation,
        MethodDeclaration,
        ElementAccess,
        MemberAccess,
        Project,
        ProjectFile,
        ProjectType
    }
    public enum ProjectType
    {
        WebApi,
        Mvc,
        WebClassLibrary,
        ClassLibrary,
        CoreWebApi,
        CoreMvc,
        WCFConfigBasedService,
        WCFCodeBasedService,
        WCFClient,
        WCFServiceLibrary,
        MonolithService,
        WebForms,
        VBNetMvc,
        VBWebForms,
        VBWebApi,
        VBClassLibrary
    }
    public enum FileTypeCreation
    {
        Startup,
        Program,
        MonolithService,
        MonolithServiceMvc,
        MonolithServiceCore
    }
    public enum WebServerConfigAttributes
    {
        Authorization,
        Authentication,
        Modules,
        Handlers,
        Security,
        HttpCompression,
        HttpRedirect,
        AnonymousAuthentication,
        WindowsAuthentication,
        FormsAuthentication,
        BasicAuthentication,
        DigestAuthentication,
        RequestFiltering,
        RequestLimits
    }

    public enum WebFormsActionType
    {
        DirectiveConversion,
        ControlConversion,
        ClassConversion,
        FileConversion
    }

    public enum ProjectLanguage
    {
        VisualBasic,
        Csharp,
    }

    public class SupportedFrameworks
    {
        public const string Netcore31 = "netcoreapp3.1";
        public const string Net5 = "net5.0";
        public const string Net6 = "net6.0";
        public const string Net7 = "net7.0";

        //Needs to be a constant since this is used at compile time for CTA.Rules.PortCore\PortCoreRulesCli.cs
        public const string SupportedFrameworksString = Netcore31 + ", " + Net5 + ", " + Net6 + ", " + Net7;

        public static List<string> GetSupportedFrameworksList()
        {
            var supportedFrameworksList = new List<string>();
            foreach (var prop in new SupportedFrameworks().GetType().GetFields())
            {
                if(prop.Name != "SupportedFrameworksString")
                    supportedFrameworksList.Add(prop.GetValue(prop) as string);
            }
            return supportedFrameworksList;
        }
    }

    public class SupportedCPUs
    {
        public const string x86 = "x86";
        public const string x64 = "x64";
        public const string ARM64 = "ARM64";

        public static List<string> GetSupportedCPUsList()
        {
            var supportedcpus = new List<string>();
            foreach (var prop in new SupportedCPUs().GetType().GetFields())
            {
                supportedcpus.Add(prop.GetValue(prop) as string);
            }
            return supportedcpus;
        }
    }

    public static class FileExtension
    {
        public static readonly string VisualBasic = ".vb";
        public static readonly string CSharp = ".cs";
        public static readonly string Backup = ".bak";
    }
}
