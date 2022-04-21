using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private const string RuleContributionsSolutionFileName = "RuleContributions.sln";
    private const string RuleContributionsProjectFileName = "RuleContributions.csproj";
    private string _tempDir = "";
    private Dictionary<string, TestSolutionAnalysis> _resultsDict;

    [OneTimeSetUp]
    public void Setup()
    {
        _tempDir = SetupTests.TempDir;

        var solutionPath = CopySolutionFolderToTemp(RuleContributionsSolutionFileName, _tempDir);
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
    public void Porting_With_All_Contributed_Rules_Results_In_Zero_Build_Errors(string version)
    {
        var solutionPortingResult = _resultsDict[version];
        CollectionAssert.IsEmpty(solutionPortingResult.SolutionRunResult.BuildErrors);
    }

    [TestCase(TargetFramework.DotnetCoreApp31)]
    [TestCase(TargetFramework.Dotnet5)]
    [TestCase(TargetFramework.Dotnet6)]
    public void DynamicQuery_Package_Is_Added_And_Namespaces_Are_Replaced(string version)
    {
        var csFileName = "System.Linq.Dynamic.cs";

        var solutionPortingResult = _resultsDict[version];
        var ruleContributionsResult = solutionPortingResult.ProjectResults.First(proj => proj.CsProjectPath.EndsWith(RuleContributionsProjectFileName));
        var ruleContributionsFile = Directory.EnumerateFiles(ruleContributionsResult.ProjectDirectory, "*.cs", SearchOption.AllDirectories)
            .First(file => file.EndsWith(csFileName));
        var ruleContributionsFileContent = File.ReadAllText(ruleContributionsFile);

        // Verify expected package is in .csproj
        StringAssert.Contains(@"PackageReference Include=""System.Linq.Dynamic.Core""", ruleContributionsResult.CsProjectContent);

        // Verify namespaces were updated
        StringAssert.Contains("using System.Linq.Dynamic.Core;", ruleContributionsFileContent);
        StringAssert.DoesNotContain("using System.Linq.Dynamic;", ruleContributionsFileContent);
    }
    
    [TestCase(TargetFramework.DotnetCoreApp31)]
    [TestCase(TargetFramework.Dotnet5)]
    [TestCase(TargetFramework.Dotnet6)]
    public void BouncyCastleNetCore_Package_Is_Added(string version)
    {
        var solutionPortingResult = _resultsDict[version];
        var ruleContributionsResult = solutionPortingResult.ProjectResults.First(proj => proj.CsProjectPath.EndsWith(RuleContributionsProjectFileName));

        // Verify expected package is in .csproj
        StringAssert.Contains(@"PackageReference Include=""BouncyCastle.NetCore""", ruleContributionsResult.CsProjectContent);
    }

    [TestCase(TargetFramework.DotnetCoreApp31)]
    [TestCase(TargetFramework.Dotnet5)]
    [TestCase(TargetFramework.Dotnet6)]
    public void NPOI_Package_Is_Added(string version)
    {
        var solutionPortingResult = _resultsDict[version];
        var ruleContributionsResult = solutionPortingResult.ProjectResults.First(proj => proj.CsProjectPath.EndsWith(RuleContributionsProjectFileName));

        // Verify expected package is in .csproj
        StringAssert.Contains(@"PackageReference Include=""NPOI""", ruleContributionsResult.CsProjectContent);
    }
} 