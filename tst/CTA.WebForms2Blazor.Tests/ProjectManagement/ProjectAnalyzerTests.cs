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
            var projectAnalyzer = new ProjectAnalyzer(PartialProjectSetupFixture.TestStructure1Path);
            var projectFileInfo = projectAnalyzer.GetProjectFileInfo();

            Assert.AreEqual(projectFileInfo.Count(), 3);
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.TEST_XML_FILE_NAME)));
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.NESTED_TEST_CLASS_FILE_NAME)));
            Assert.True(projectFileInfo.Any(fileInfo => fileInfo.Name.Equals(PartialProjectSetupFixture.NESTED_TEST_TEXT_FILE_NAME)));
        }
    }
}
