using System;
using System.Collections.Generic;
using CTA.Rules.Config;

namespace CTA.Rules.Models
{
    public class ProjectConfiguration
    {
        public ProjectConfiguration()
        {
            TargetVersions = new List<string> { Constants.DefaultCoreVersion };
            PackageReferences = new Dictionary<string, Tuple<string, string>>();
        }
        public string ProjectPath;
        public List<string> TargetVersions;
        public string AssemblyDir;
        public string RulesPath;
        public Dictionary<string, Tuple<string, string>> PackageReferences;
        public bool IsMockRun = false;
        public ProjectType ProjectType = ProjectType.ClassLibrary;
    }
}
