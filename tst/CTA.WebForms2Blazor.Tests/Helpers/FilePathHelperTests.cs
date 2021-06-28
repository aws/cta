using CTA.WebForms2Blazor.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CTA.WebForms2Blazor.Tests.Helpers
{
    public class FilePathHelperTests
    {
        [Test]
        public void AlterFileName_Does_Not_Modify_Leading_Paths()
        {
            var leadingPath = Path.Combine("C:", "Dir1", "Dir2");
            var inputString = Path.Combine(leadingPath, "FileName.txt");
            var outputString = FilePathHelper.AlterFileName(inputString, newFileName: "NewFileName");

            Assert.True(outputString.StartsWith(leadingPath));
        }

        [Test]
        public void AlterFileName_Properly_Replaces_Extension_When_File_Name_Not_Changed()
        {
            var inputFileName = "FileName.aspx.cs";
            var expectedOutputFileName = "FileName.razor";
            var actualOutputFileName = FilePathHelper.AlterFileName(inputFileName, oldExtension: ".aspx.cs", newExtension: ".razor");

            Assert.AreEqual(expectedOutputFileName, actualOutputFileName);
        }

        [Test]
        public void AlterFileName_Properly_Replaces_File_Name_When_Extension_Not_Changed()
        {
            var inputFileName = "FileName1.aspx.cs";
            var expectedOutputFileName = "FileName2.aspx.cs";
            var actualOutputFileName = FilePathHelper.AlterFileName(inputFileName, newFileName: "FileName2", oldExtension: ".aspx.cs");

            Assert.AreEqual(expectedOutputFileName, actualOutputFileName);
        }

        [Test]
        public void AlterFileName_Works_Properly_When_Name_And_Extension_Are_Changed()
        {
            var inputFileName = "FileName1.aspx.cs";
            var expectedOutputFileName = "FileName2.razor";
            var actualOutputFileName = FilePathHelper.AlterFileName(inputFileName,
                newFileName: "FileName2",
                oldExtension: ".aspx.cs",
                newExtension: ".razor");

            Assert.AreEqual(expectedOutputFileName, actualOutputFileName);
        }
    }
}
