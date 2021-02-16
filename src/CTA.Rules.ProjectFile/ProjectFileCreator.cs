using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CTA.Rules.Config;
using CTA.Rules.Models;

namespace CTA.Rules.ProjectFile
{
    public class ProjectFileCreator
    {
        private readonly string _projectFile;

        private readonly List<string> _targetVersions;
        private Dictionary<string, string> _packages;
        private IEnumerable<string> _projectReferences;
        private ProjectType _projectType;


        private readonly string csCoreProjSyntaxWeb = @"
                <Project Sdk=""{0}"">
                    <PropertyGroup>
                        <TargetFramework>{1}</TargetFramework>
                    </PropertyGroup>
                    {2}
                </Project>";

        private readonly string csCoreProjSyntaxWebClassLibrary = @"
                <Project Sdk=""{0}"">
                    <PropertyGroup>
                        <TargetFramework>{1}</TargetFramework>
                    </PropertyGroup>
                    <ItemGroup>
                        <FrameworkReference Include=""Microsoft.AspNetCore.App"" />
                    </ItemGroup>
                    {2}
                </Project>";

        private readonly string csCoreProjSyntaxClassLibrary = @"
                <Project Sdk=""{0}"">
                    <PropertyGroup>
                        <TargetFramework>{1}</TargetFramework>
                    </PropertyGroup>
                    {2}
                </Project>";

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
        /// <param name="projectFile">Current project file</param>
        public bool Create()
        {
            try
            {
                string sdkName = Constants.ClassLibrarySdkName;
                string csProj = csCoreProjSyntaxClassLibrary;

                if (_projectType == ProjectType.Mvc || _projectType == ProjectType.WebApi)
                {
                    csProj = csCoreProjSyntaxWeb;
                    sdkName = Constants.WebSdkName;
                }
                else if (_projectType == ProjectType.WebClassLibrary)
                {
                    csProj = csCoreProjSyntaxWebClassLibrary;
                }

                string packages = GetPackagesSection();
                string projects = GetProjectReferencesSection();

                string csProjContent = string.Format(csProj, sdkName, GetTargetVersions(), string.Concat(AddItemGroup(packages), AddItemGroup(projects)));
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
            if (!string.IsNullOrEmpty(content))
            {
                return string.Format(@"
                <ItemGroup>
                    {0}
                </ItemGroup>", content);
            }
            return string.Empty;
        }

        private string GetPackagesSection()
        {
            string packageSyntax = @"<PackageReference Include=""{0}"" Version=""{1}"" />";
            StringBuilder str = new StringBuilder();
            foreach (var action in _packages)
            {
                str.Append(string.Format(packageSyntax, action.Key, action.Value)).Append(Environment.NewLine);
            }
            if (_packages.Count() > 0)
            {
                LogChange(string.Format("Adding reference to packages {0}", string.Join(",", _packages.Select(p => p.Key))));
            }
            return str.ToString();
        }

        private string GetProjectReferencesSection()
        {
            List<string> references = new List<string>();
            string projectReferenceTemplate = @"<ProjectReference Include=""{0}"" />";

            if (_projectReferences != null)
            {
                foreach (var projectReference in _projectReferences)
                {
                    references.Add(string.Format(projectReferenceTemplate, projectReference));
                }
            }

            return string.Join(Environment.NewLine, references);
        }

        private void LogChange(string message)
        {
            LogHelper.LogInformation(message);
        }

        private void PopulateFromExistingFile()
        {
            var xDocument = XDocument.Load(_projectFile);
            var packages = new Dictionary<string, string>();

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
                packages = xDocument.Descendants()
                    .Where(d => d.Name == "PackageReference")
                    .ToDictionary(p => p.Attributes("Include").First().Value, p => p.Attributes("Version").First().Value);

                _packages = packages.Union(_packages).ToDictionary(d => d.Key, d => d.Value);
            }
            catch (Exception)
            {
                //If we're using a framework csproj, we will get an error. No need to catch since we're overwriting the csproj file
            }
            var projects = new List<string>();

            try
            {
                projects = xDocument.Descendants()
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


    }
}
