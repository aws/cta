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
        private readonly string _projectFile;
        private readonly List<string> _targetVersions;
        private readonly List<string> _sourceVersions;
        private readonly List<string> _metaReferences;
        private Dictionary<string, string> _packages;
        private IEnumerable<string> _projectReferences;
        private ProjectType _projectType;
        private XDocument _projectFileXml;

        private readonly string _csCoreProjSyntaxWeb =
@"<Project Sdk=""{0}"">
  <PropertyGroup>
    <TargetFramework>{1}</TargetFramework>
  </PropertyGroup>
{2}
{3}
{4}
</Project>";

        private readonly string _csCoreProjSyntaxWebClassLibrary =
@"<Project Sdk=""{0}"">
  <PropertyGroup>
    <TargetFramework>{1}</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include=""Microsoft.AspNetCore.App"" />
  </ItemGroup>
{2}
{3}
{4}
</Project>";

        private readonly string _csCoreProjSyntaxClassLibrary =
@"<Project Sdk=""{0}"">
  <PropertyGroup>
    <TargetFramework>{1}</TargetFramework>
  </PropertyGroup>
{2}
{3}
{4}
</Project>";

        private readonly string _csCoreProjExe =
@"<Project Sdk=""{0}"">
  <PropertyGroup>
    <TargetFramework>{1}</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
{2}
{3}
{4}
</Project>";

        private readonly string _itemGroupTemplate =
@"<ItemGroup>
{0}
</ItemGroup>";

        private readonly string _portingInfoItemGroupTemplate =
@"<ItemGroup Label=""PortingInfo"">
<!-- DO NOT REMOVE WHILE PORTING
{0}
-->
</ItemGroup>";

        private const string PackageReferenceTemplate = @"<PackageReference Include=""{0}"" Version=""{1}"" />";
        private const string ProjectReferenceTemplate = @"<ProjectReference Include=""{0}"" />";
        private const string IndentationPerLevel = "  ";

        public ProjectFileCreator(
            string projectFile,
            List<string> targetVersions, 
            Dictionary<string, string> packages,
            List<string> projectReferences, 
            ProjectType projectType, 
            List<string> metaReferences)
        {
            _projectFile = projectFile;
            _targetVersions = targetVersions;
            _packages = packages;
            _projectReferences = projectReferences;
            _projectType = projectType;
            _metaReferences = metaReferences;

            try
            {
                _projectFileXml = XDocument.Load(projectFile);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error initializing project file");
                throw;
            }
        }

        public ProjectFileCreator(
            string projectFile, 
            List<string> targetVersions, 
            Dictionary<string, string> packages,
            List<string> projectReferences, 
            ProjectType projectType, 
            List<string> metaReferences, 
            List<string> sourceVersions) 
                : this(projectFile, targetVersions, packages, projectReferences, projectType, metaReferences)
        {
            _sourceVersions = sourceVersions;
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

        public string CreateContents()
        {
            string csProjContent = String.Empty;
            try
            {
                string sdkName = Constants.ClassLibrarySdkName;
                string csProj = _csCoreProjSyntaxClassLibrary;

                if (_projectType == ProjectType.Mvc || _projectType == ProjectType.WebApi || _projectType == ProjectType.WebForms)
                {
                    csProj = _csCoreProjSyntaxWeb;
                    sdkName = Constants.WebSdkName;
                }
                else if (_projectType == ProjectType.WebClassLibrary)
                {
                    csProj = _csCoreProjSyntaxWebClassLibrary;
                }
                else if (_projectType == ProjectType.WCFCodeBasedService || _projectType == ProjectType.WCFConfigBasedService)
                {
                    csProj = _csCoreProjExe;
                }

                string packages = GetPackagesSection();
                string projects = GetProjectReferencesSection();
                string metaReferences = GetMetaReferencesSection();

                if (IsCoreProject() == true)
                {
                    UpdateExistingFile();
                    _projectFileXml.Save(_projectFile);
                }
                else
                {
                    csProjContent = string.Format(csProj, sdkName, GetTargetVersions(), AddItemGroup(packages), AddItemGroup(projects), AddItemGroup(metaReferences, _portingInfoItemGroupTemplate));
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while creating project file for {0}", _projectFile);
                throw new Exception($"Error while creating project file for [{_projectFile}]: {ex.Message}", ex);
            }

            return csProjContent;
        }

        //TODO Get project references and add to new project
        /// <summary>
        /// Replaces the current project file with a new file 
        /// </summary>
        public bool Create()
        {
            string csProjContent = CreateContents();
            if (!String.IsNullOrEmpty(csProjContent))
            {
                File.WriteAllText(_projectFile, csProjContent);
                return true;
            }

            if (csProjContent.Equals(String.Empty))
            {
                return true;
            }
            
            return false;
        }

        private string GetMetaReferencesSection() => String.Join(Environment.NewLine, _metaReferences);

        private string AddItemGroup(string content, string itemGroupTemplate = null)
        {
            if (string.IsNullOrEmpty(content?.Trim()))
            {
                return string.Empty;
            }

            var currentGroupTemplate = itemGroupTemplate ?? _itemGroupTemplate;

            var itemGroupContent = string.Format(currentGroupTemplate, content);
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

        private bool? IsCoreProject() =>
            _projectType == ProjectType.CoreMvc
            || _projectType == ProjectType.CoreWebApi
            || _projectFileXml.Descendants().Any(d => d.Name?.LocalName == "TargetFramework" && (d.Value?.Split(';').Intersect(SupportedFrameworks.GetSupportedFrameworksList()).Any()) == true)
            || _projectFileXml.Descendants().Any(d => d.Name?.LocalName == "TargetFrameworks" && (d.Value?.Split(';').Intersect(SupportedFrameworks.GetSupportedFrameworksList()).Any()) == true);



        private void UpdateExistingFile()
        {
            UpdateVersion();
            UpdatePackageReferences();
        }

        private void UpdateVersion()
        {
            var currentVersionTag = _projectFileXml.Descendants().FirstOrDefault(d => d.Name == "TargetFramework");
            if(currentVersionTag == null)
            {
                currentVersionTag = _projectFileXml.Descendants().FirstOrDefault(d => d.Name == "TargetFrameworks");
            }

            var currentVersion = currentVersionTag.Value;
            var targetVersion = GetTargetVersions();
            if (!targetVersion.Equals(currentVersion))
            {
                currentVersionTag.Value = targetVersion;
            }
        }

        private void UpdatePackageReferences()
        {
            //Select existing packages and deduplicate them by package name:
            var existingPackages = _projectFileXml.Descendants()
                .Where(d => d.Name == "PackageReference")
                .Select(d => new
                {
                    Name = d.Attributes("Include")
                        .FirstOrDefault()?
                        .Value ?? "",
                    Version = d.Attributes("Version")
                        .FirstOrDefault()?
                        .Value ?? ""
                })
                .GroupBy(d => d.Name)
                .Select(d => d.FirstOrDefault())
                .ToDictionary(p => p.Name,
                    p => p.Version);

            _packages = _packages.Where(p => existingPackages.Keys?.Contains(p.Key) == false).ToDictionary(d => d.Key, d => d.Value);

            //No packages to add
            if (_packages.Count == 0) return;

            var packages = GetPackagesSection();
            if (existingPackages.Count == 0)
            {
                packages = AddItemGroup(packages);

                var existingItemGroup = _projectFileXml.Descendants().LastOrDefault(d => d.Name == "ItemGroup");

                // add after existing <ItemGroup> sections
                if (null != existingItemGroup)
                    existingItemGroup.AddAfterSelf(XElement.Parse(packages));

                // otherwise, just stick it at the end of the document
                if (_projectFileXml.Descendants().Any())
                    _projectFileXml.Descendants().Last().AddAfterSelf(XElement.Parse(packages));
                else
                    _projectFileXml.Add(XElement.Parse(packages));
            }
            else
            {
                _projectFileXml.Descendants().Last(d => d.Name == "PackageReference").AddAfterSelf(XElement.Parse(packages));
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
