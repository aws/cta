using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CTA.Rules.Config;
using CTA.Rules.Models;

namespace CTA.Rules.ProjectFile
{
    public class ProjectFileCreator
    {
        private string _projectFile;
        private List<string> _targetVersions;
        private Dictionary<string, string> _packages;
        private IEnumerable<string> _projectReferences;
        private ProjectType _projectType;

        private const string CsCoreProjSyntaxWeb = 
@"<Project Sdk=""{0}"">
  <PropertyGroup>
    <TargetFramework>{1}</TargetFramework>
  </PropertyGroup>
{2}
{3}
</Project>";

        private const string CsCoreProjSyntaxWebClassLibrary =
@"<Project Sdk=""{0}"">
  <PropertyGroup>
    <TargetFramework>{1}</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include=""Microsoft.AspNetCore.App"" />
  </ItemGroup>
{2}
{3}
</Project>";

        private const string CsCoreProjSyntaxClassLibrary =
@"<Project Sdk=""{0}"">
  <PropertyGroup>
    <TargetFramework>{1}</TargetFramework>
  </PropertyGroup>
{2}
{3}
</Project>";

        private const string ItemGroupTemplate =
@"<ItemGroup>
{0}
</ItemGroup>";

        private const string PackageReferenceTemplate = @"<PackageReference Include=""{0}"" Version=""{1}"" />";
        private const string ProjectReferenceTemplate = @"<ProjectReference Include=""{0}"" />";
        private const string IndentationPerLevel = "  ";

        public ProjectFileCreator(string projectFile, List<string> targetVersions, Dictionary<string, string> packages,
            List<string> projectReferences, ProjectType projectType)
        {
            _projectFile = projectFile;
            _targetVersions = targetVersions;
            _packages = packages;
            _projectReferences = projectReferences;
            _projectType = projectType;
            PopulateFromExistingFile();
        }

        private string GetTargetVersions()
        {
            if (_targetVersions.Count > 1)
            {
                LogHelper.LogDebug("Only one framework version is supported at this time. {0} was used", _targetVersions[0]);
            }

            if (_targetVersions.Count > 0)
            {
                return _targetVersions[0];
            }
            return Constants.DefaultCoreVersion;
        }

        //TODO Get project references and add to new project
        /// <summary>
        /// Replaces the current project file with a new file 
        /// </summary>
        public bool Create()
        {
            try
            {
                string sdkName = Constants.ClassLibrarySdkName;
                string csProj = CsCoreProjSyntaxClassLibrary;

                if (_projectType == ProjectType.Mvc || _projectType == ProjectType.WebApi)
                {
                    csProj = CsCoreProjSyntaxWeb;
                    sdkName = Constants.WebSdkName;
                }
                else if (_projectType == ProjectType.WebClassLibrary)
                {
                    csProj = CsCoreProjSyntaxWebClassLibrary;
                }

                string packages = GetPackagesSection();
                string projects = GetProjectReferencesSection();

                string csProjContent = string.Format(csProj, sdkName, GetTargetVersions(), AddItemGroup(packages), AddItemGroup(projects));
                File.WriteAllText(_projectFile, csProjContent);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while creating project file for {0}", _projectFile);
                return false;
            }
            return true;
        }

        private string AddItemGroup(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }
            
            var itemGroupContent = string.Format(ItemGroupTemplate, content);
            itemGroupContent = IndentAllLines(itemGroupContent);

            return itemGroupContent;
        }

        private string GetPackagesSection()
        {
            IEnumerable<string> packages = new List<string>();
            packages = _packages.Select(package =>
            {
                var packageName = package.Key;
                var packageVersion = package.Value;

                return string.Format(PackageReferenceTemplate, packageName, packageVersion);
            });

            if (_packages.Any())
            {
                LogChange(string.Format("Adding reference to packages {0}", string.Join(",", _packages.Select(p => p.Key))));
            }
            
            var content = string.Join(Environment.NewLine, packages);
            content = IndentAllLines(content);
            return content;
        }

        private string GetProjectReferencesSection()
        {
            IEnumerable<string> references = new List<string>();
            if (_projectReferences != null)
            {
                references = _projectReferences.Select(projectReference => 
                    string.Format(ProjectReferenceTemplate, projectReference));
            }

            var content = string.Join(Environment.NewLine, references);
            content = IndentAllLines(content);

            return content;
        }

        private void LogChange(string message)
        {
            LogHelper.LogInformation(message);
        }

        private void PopulateFromExistingFile()
        {
            var xDocument = XDocument.Load(_projectFile);

            try
            {
                var sdk = xDocument.Descendants()?.First()?.Attribute("Sdk")?.Value;
                if (sdk == Constants.WebSdkName)
                {
                    if (_projectType == ProjectType.ClassLibrary || _projectType == ProjectType.WebClassLibrary)
                    {
                        _projectType = ProjectType.Mvc;
                    }
                }
            }
            catch (Exception)
            {
                //If we're using a framework csproj, we will get an error. No need to catch since we're overwriting the csproj file
            }

            try
            {
                var packages = xDocument.Descendants()
                    .Where(d => d.Name == "PackageReference")
                    .ToDictionary(p => p.Attributes("Include").First().Value, p => p.Attributes("Version").First().Value);

                _packages = packages.Union(_packages).ToDictionary(d => d.Key, d => d.Value);
            }
            catch (Exception)
            {
                //If we're using a framework csproj, we will get an error. No need to catch since we're overwriting the csproj file
            }

            try
            {
                var projects = xDocument.Descendants()
                    .Where(d => d.Name == "ProjectReference")
                    .Select(p => p.Attributes("Include").First().Value).ToList();

                _projectReferences = projects.Union(_projectReferences).Distinct().ToList();
            }
            catch (Exception)
            {
                //If we're using a framework csproj, we will get an error. No need to catch since we're overwriting the csproj file
            }

            try
            {
                if (_projectType == ProjectType.ClassLibrary && xDocument.Descendants().Where(d => d.Name == "FrameworkReference").Any())
                {
                    _projectType = ProjectType.WebClassLibrary;
                }
            }
            catch (Exception)
            {
                //If we're using a framework csproj, we will get an error. No need to catch since we're overwriting the csproj file
            }
        }

        private static string IndentAllLines(string content)
        {
            var lines = content.Split(Environment.NewLine);
            var indentedLines = lines.Select(line => $"{IndentationPerLevel}{line}");

            return string.Join(Environment.NewLine, indentedLines);
        }
    }
}
