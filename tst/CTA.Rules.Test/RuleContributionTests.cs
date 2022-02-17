using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CTA.Rules.Test;

/// <summary>
/// This class is exclusively for testing RuleContributions.sln,
/// a solution with a .NET Framework project containing all supported
/// rules contributed by third parties.
/// </summary>
[TestFixture]
class RuleContributionTests : AwsRulesBaseTest
{
    private string _tempDir = "";
    private string _downloadLocation;
    private Dictionary<string, TestSolutionAnalysis> _resultsDict;

    [OneTimeSetUp]
    public void Setup()
    {
        _tempDir = SetupTests.TempDir;
        _downloadLocation = SetupTests.DownloadLocation;

        var solutionName = "RuleContributions.sln";
        var solutionPath = CopySolutionFolderToTemp(solutionName, _tempDir);
        var net31Results = AnalyzeSolution(solutionPath, TargetFramework.DotnetCoreApp31);
        var net50Results = AnalyzeSolution(solutionPath, TargetFramework.Dotnet5);
        var net60Results = AnalyzeSolution(solutionPath, TargetFramework.Dotnet6);

        _resultsDict = new Dictionary<string, TestSolutionAnalysis>
        {
            {TargetFramework.DotnetCoreApp31, net31Results},
            {TargetFramework.Dotnet5, net50Results},
            {TargetFramework.Dotnet6, net60Results}
        };
    }
    
    [TestCase(TargetFramework.DotnetCoreApp31)]
    [TestCase(TargetFramework.Dotnet5)]
    [TestCase(TargetFramework.Dotnet6)]
    public void TestProjectFilePortingResults(string version)
    {
        var projectFileName = "RuleContributions.csproj";
        var csFileName = "System.Linq.Dynamic.cs";

        var solutionPortingResult = _resultsDict[version];
        var ruleContributionsResult = solutionPortingResult.ProjectResults.First(proj => proj.CsProjectPath.EndsWith(projectFileName));
        var ruleContributionsFile = Directory.EnumerateFiles(ruleContributionsResult.ProjectDirectory, "*.cs", SearchOption.AllDirectories)
            .First(file => file.EndsWith(csFileName));
        var ruleContributionsFileContent = File.ReadAllText(ruleContributionsFile);

        // Verify there are no build errors after porting
        CollectionAssert.IsEmpty(solutionPortingResult.SolutionRunResult.BuildErrors);

        // Verify expected package is in .csproj
        StringAssert.Contains(@"PackageReference Include=""System.Linq.Dynamic.Core""", ruleContributionsResult.CsProjectContent);

        // Verify namespaces were updated
        StringAssert.Contains("using System.Linq.Dynamic.Core;", ruleContributionsFileContent);
        StringAssert.DoesNotContain("using System.Linq.Dynamic;", ruleContributionsFileContent);
    }
} 