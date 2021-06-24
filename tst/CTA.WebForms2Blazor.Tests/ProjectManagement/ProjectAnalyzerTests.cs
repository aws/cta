using System.Linq;
using CTA.WebForms2Blazor.ProjectManagement;
using NUnit.Framework;

namespace CTA.WebForms2Blazor.Tests.ProjectManagement
{
    public class ProjectAnalyzerTests
    {
        [Test]
        public void GetProjectFileInfo_Retrieves_Files_From_All_Directory_Levels()
        {
            var projectAnalyzer = new ProjectAnalyzer(PartialProjectSetupFixture.TestStructure1Path, null);
            var projectFileInfo = projectAnalyzer.GetProjectFileInfo();

            Assert.AreEqual(projectFileInfo.Count(), 3);
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.TestXMLFileName)));
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.NestedTestClassFileName)));
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.NestedTestTextFileName)));
        }
    }
}
