using CTA.WebForms.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CTA.WebForms.Tests.Helpers
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

        [Test]
        public void RemoveDuplicateDirectories_Removes_Single_Duplicate()
        {
            var inputPath = Path.Combine("Dir1", "Dir2", "Dir2", "Dir3");
            var expectedPath = Path.Combine("Dir1", "Dir2", "Dir3");

            Assert.AreEqual(expectedPath, FilePathHelper.RemoveDuplicateDirectories(inputPath));
        }

        [Test]
        public void RemoveDuplicateDirectories_Removes_Multiple_Duplicates()
        {
            var inputPath = Path.Combine("Dir1", "Dir2", "Dir2", "Dir2", "Dir2", "Dir3");
            var expectedPath = Path.Combine("Dir1", "Dir2", "Dir3");

            Assert.AreEqual(expectedPath, FilePathHelper.RemoveDuplicateDirectories(inputPath));
        }

        [Test]
        public void IsSubDirectory_Returns_True_If_OtherPath_Is_One_Level_Above_BasePath()
        {
            var basePath = Path.Combine("Dir1", "Dir2");
            var otherPath = Path.Combine(basePath, "Dir3");

            Assert.True(FilePathHelper.IsSubDirectory(basePath, otherPath));
        }

        [Test]
        public void IsSubDirectory_Returns_True_If_OtherPath_Is_Multiple_Levels_Above_BasePath()
        {
            var basePath = Path.Combine("Dir1", "Dir2");
            var otherPath = Path.Combine(basePath, "Dir3", "Dir4", "Dir5");

            Assert.True(FilePathHelper.IsSubDirectory(basePath, otherPath));
        }

        [Test]
        public void IsSubDirectory_Returns_False_If_OtherPath_Is_Same_As_BasePath()
        {
            var path = Path.Combine("Dir1", "Dir2");

            Assert.False(FilePathHelper.IsSubDirectory(path, path));
        }

        [Test]
        public void IsSubDirectory_Returns_False_If_OtherPath_Is_One_Level_Below_BasePath()
        {
            var otherPath = Path.Combine("Dir1", "Dir2");
            var basePath = Path.Combine(otherPath, "Dir3");

            Assert.False(FilePathHelper.IsSubDirectory(basePath, otherPath));
        }

        [Test]
        public void IsSubDirectory_Returns_False_If_OtherPath_Is_Multiple_Levels_Below_BasePath()
        {
            var otherPath = Path.Combine("Dir1", "Dir2");
            var basePath = Path.Combine(otherPath, "Dir3", "Dir4", "Dir5");

            Assert.False(FilePathHelper.IsSubDirectory(basePath, otherPath));
        }

        [Test]
        public void IsSubDirectory_Returns_False_If_Paths_Do_Not_Share_A_Root()
        {
            var basePath = Path.Combine("Dir1Alt", "Dir2", "Dir3");
            var otherPath = Path.Combine("Dir1", "Dir2");

            Assert.False(FilePathHelper.IsSubDirectory(basePath, otherPath));
        }
    }
}
