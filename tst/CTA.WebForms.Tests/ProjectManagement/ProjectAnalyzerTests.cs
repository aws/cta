﻿using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.Rules.Models;
using CTA.WebForms.ProjectManagement;
using NUnit.Framework;

namespace CTA.WebForms.Tests.ProjectManagement
{
    public class ProjectAnalyzerTests
    {
        [Test]
        public void GetProjectFileInfo_Retrieves_Files_From_All_Directory_Levels()
        {
            var projectAnalyzer = new ProjectAnalyzer(PartialProjectSetupFixture.TestStructure1Path, new AnalyzerResult(), new PortCoreConfiguration(), new ProjectResult());
            var projectFileInfo = projectAnalyzer.GetProjectFileInfo();

            Assert.AreEqual(projectFileInfo.Count(), 3);
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.TestXMLFileName)));
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.NestedTestClassFileName)));
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.NestedTestTextFileName)));
        }
    }
}
