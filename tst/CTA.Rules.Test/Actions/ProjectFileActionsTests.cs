using Amazon.Runtime.Internal.Transform;
using CTA.Rules.Actions;
using CTA.Rules.Config;
using CTA.Rules.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CTA.Rules.Test.Actions
{
    public class ProjectFileActionsTests
    {
        private string _projectDir;
        private string _projectFile;
        private ProjectFileActions _projectFileActions;

        [SetUp]
        public void SetUp()
        {
            _projectDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _projectFile = Path.Combine(_projectDir, "sample.csproj");

            _projectFileActions = new ProjectFileActions();
        }

        [Test]
        public void ProjectFileCreationNoVersion()
        {
            var result = CreateNewFile(ProjectType.Mvc, new List<string>(), new Dictionary<string, string>(), new List<string>());
            StringAssert.Contains(Constants.DefaultCoreVersion, result);
        }

        [Test]
        public void ProjectFileCreationMvcOneVersion()
        {
            var result = CreateNewFile(ProjectType.Mvc, new List<string>() { SupportedFrameworks.Net5 }, new Dictionary<string, string>(), new List<string>());
            StringAssert.Contains(SupportedFrameworks.Net5, result);
        }

        [Test]
        public void ProjectFileCreationMvcTwoVersions()
        {
            var result = CreateNewFile(ProjectType.Mvc, new List<string>() { SupportedFrameworks.Netcore31, SupportedFrameworks.Net5 }, new Dictionary<string, string>(), new List<string>());
            StringAssert.Contains(SupportedFrameworks.Netcore31, result);
        }

        [Test]
        public void ProjectFileCreationWebClassLibrary()
        {
            var result = CreateNewFile(ProjectType.WebClassLibrary, new List<string>() { SupportedFrameworks.Net5 }, new Dictionary<string, string>(), new List<string>());
            StringAssert.Contains(SupportedFrameworks.Net5, result);
            StringAssert.Contains("Microsoft.AspNetCore.App", result);
        }

        [Test]
        public void ProjectFileCreationNoPackages()
        {
            var result = CreateNewFile(
                ProjectType.CoreMvc, 
                new List<string>() { SupportedFrameworks.Net8 },
                new Dictionary<string, string>(), new List<string>(),
                isNetCore: true);
            StringAssert.Contains(SupportedFrameworks.Net8, result);
        }

        [Test]
        public void ProjectFileCreationHandleExistingPackages()
        {
            var result = CreateNewFile(
                ProjectType.CoreMvc,
                new List<string>() { SupportedFrameworks.Net8 },
                new Dictionary<string, string>()
                {
                    {"Microsoft.EntityFrameworkCore.Design", "2.2.6"}
                }, new List<string>(),
                isNetCore: true);
            StringAssert.Contains(SupportedFrameworks.Net8, result);
        }

        [Test]
        public void ProjectFileCreationAddsPackages()
        {
            var result = CreateNewFile(
                ProjectType.CoreMvc,
                new List<string>() { SupportedFrameworks.Net8 },
                new Dictionary<string, string>()
                {
                    {"Newtonsoft.Json", "2.2.6"}
                }, new List<string>(),
                isNetCore: true);
            StringAssert.Contains(SupportedFrameworks.Net8, result);
            //verify both new and old package reference remain
            StringAssert.Contains("<PackageReference Include=\"Newtonsoft.Json\" Version=\"2.2.6\" />", result);
            StringAssert.Contains("<PackageReference Include=\"Microsoft.EntityFrameworkCore.Design\" Version=\"2.2.6\" />", result);
        }

        [Test]
        public void ProjectFileCreationHandlesNoExistingPackages()
        {
            const string projectFile = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
</Project>";

            var result = CreateNewFile(
                ProjectType.CoreMvc,
                new List<string>() { SupportedFrameworks.Net8 },
                new Dictionary<string, string>()
                {
                    {"Newtonsoft.Json", "2.2.6"}
                }, new List<string>(),
                newContent: projectFile);
            StringAssert.Contains(SupportedFrameworks.Net8, result);
            //verify both new and old package reference remain
            StringAssert.Contains("<PackageReference Include=\"Newtonsoft.Json\" Version=\"2.2.6\" />", result);
        }

        private string CreateNewFile(ProjectType projectType, List<string> targetVersions, Dictionary<string, string> packageReferences, List<string> projectReferences, bool isNetCore = false, string newContent="")
        {
            ResetProjectFile(newContent: newContent, isNetCore: isNetCore);
            var migrationProjectFileAction = _projectFileActions.GetMigrateProjectFileAction("");
            var metaRefs = new List<string>
            {
                @"C:\\RandomFile.dll"
            };
            var result = migrationProjectFileAction(_projectFile, projectType, targetVersions, packageReferences, projectReferences, metaRefs);
            return string.Concat(result, File.ReadAllText(_projectFile));
        }

        private void ResetProjectFile(string newContent = "", bool isNetCore = false)
        {
            if (!string.IsNullOrEmpty(newContent))
            {
                File.WriteAllText(_projectFile, newContent);
            }
            else if (isNetCore)
            {
                File.WriteAllText(_projectFile,
                    @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" Version=""2.2.6"" />
  </ItemGroup>
</Project>");
            }
            else
            {
                File.WriteAllText(_projectFile,
                                @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""Current"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
</Project>");
            }
        }
    }
}