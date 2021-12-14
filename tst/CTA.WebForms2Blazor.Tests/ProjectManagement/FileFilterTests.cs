using CTA.WebForms2Blazor.ProjectManagement;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CTA.WebForms2Blazor.Tests.ProjectManagement
{
    public class FileFilterTests
    {
        [TestCase("src{0}projectDir{0}bin{0}Debug{0}netcoreapp3.1{0}Microsoft.Build.dll")]
        [TestCase("build{0}buildfile.mycustomextension")]
        [TestCase("directory1{0}objectFile.obj")]
        [TestCase("metadata.meta")]
        public void ShouldIgnoreFileAtPath_Returns_True_For_Invalid_Files(string relativePath)
        {
            Assert.True(FileFilter.ShouldIgnoreFileAtPath(string.Format(relativePath, Path.DirectorySeparatorChar)));
        }

        [TestCase("src{0}projectDir{0}subDir{0}CodeFile.cs")]
        [TestCase("xmlFile.xml")]
        public void ShouldIgnoreFileAtPath_Returns_False_For_Valid_Files(string relativePath)
        {
            Assert.False(FileFilter.ShouldIgnoreFileAtPath(string.Format(relativePath, Path.DirectorySeparatorChar)));
        }

        [TestCase("directory1{0}name.cache", "directory1{0}name.cache{0}cachedFile.mycustomextension")]
        [TestCase("packages{0}package1.pack", "packages{0}build{0}package1.pack")]
        public void ShouldIgnoreFileAtPath_Returns_False_For_Valid_Files_Which_Also_Satisfy_Invalidation_Conditions(string invalidRelativePath, string validRelativePath)
        {
            Assert.True(FileFilter.ShouldIgnoreFileAtPath(string.Format(invalidRelativePath, Path.DirectorySeparatorChar)));
            Assert.False(FileFilter.ShouldIgnoreFileAtPath(string.Format(validRelativePath, Path.DirectorySeparatorChar)));
        }
    }
}
