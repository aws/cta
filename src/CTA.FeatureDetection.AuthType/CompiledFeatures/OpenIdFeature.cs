﻿using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Reporting;

namespace CTA.FeatureDetection.AuthType.CompiledFeatures
{
    public class OpenIdFeature : WebConfigFeature
    {
        public override FeatureCategory FeatureCategory => FeatureCategory.AuthType;

        public override string Description => "This project uses the OpenID Authentication method.";

        public override bool IsLinuxCompatible => false;

        /// <summary>
        /// Determines if OpenId Authentication is being used in a given project based on references and
        /// method invocations in code.
        ///
        /// Qualifications:
        /// 1. using DotNetOpenAuth;
        ///
        /// 2. IAppBuilder.UseOpenIdConnectAuthentication(...)
        /// 
        /// </summary>
        /// <param name="analyzerResult">Source code analysis results</param>
        /// <returns>Whether or not Federated Authentication is used</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            return analyzerResult.ProjectResult.ContainsNugetDependency(Constants.DotNetOpenAuthReferenceIdentifier)
                   || analyzerResult.ProjectResult.SourceFileResults.Any(s =>
                       s.AllInvocationExpressions().Any(i => i.SemanticOriginalDefinition?.StartsWith(Constants.OpenIdConnectAuthenticationQualifiedName) == true));
        }
    }
}
