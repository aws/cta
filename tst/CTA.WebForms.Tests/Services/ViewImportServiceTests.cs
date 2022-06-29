﻿using CTA.WebForms.Services;
using NUnit.Framework;
using System;
using System.Text;

namespace CTA.WebForms.Tests.Services
{
    public class ViewImportServiceTests
    {
        private const string ExpectedBasicImportsContent =
@"@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using System.Net.Http";
        private const string ExpectedModifiedImportsContent =
@"@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using MyCustomPackage.Custom
@using System.Net.Http";

        private ViewImportService _viewImportService;

        [SetUp]
        public void SetUp()
        {
            _viewImportService = new ViewImportService();
        }

        [Test]
        public void ConstructImportsFile_Constructs_File_Information_With_Basic_Content()
        {
            var fileInfo = _viewImportService.ConstructImportsFile();

            Assert.AreEqual(ExpectedBasicImportsContent, Encoding.UTF8.GetString(fileInfo.FileBytes));
        }

        [Test]
        public void AddViewImport_Ignores_Duplicate_Using_Directives()
        {
            _viewImportService.AddViewImport("@using Microsoft.AspNetCore.Authorization");
            _viewImportService.AddViewImport("@using System.Net.Http");
            var fileInfo = _viewImportService.ConstructImportsFile();

            Assert.AreEqual(ExpectedBasicImportsContent, Encoding.UTF8.GetString(fileInfo.FileBytes));
        }

        [Test]
        public void AddViewImport_Adds_New_Using_Directives()
        {
            _viewImportService.AddViewImport("@using MyCustomPackage.Custom");
            var fileInfo = _viewImportService.ConstructImportsFile();

            Assert.AreEqual(ExpectedModifiedImportsContent, Encoding.UTF8.GetString(fileInfo.FileBytes));
        }

        [Test]
        public void AddViewImport_Throws_Exception_On_Malformed_Directive()
        {
            Assert.Throws(typeof(ArgumentException), () => _viewImportService.AddViewImport("MyCustomPackage.Custom"));
        }

        [Test]
        public void AddNuGetPackage_Adds_New_Nuget_Package_To_Set()
        {
            var packageName = "Fritz.BlazorWebFormsComponents";

            _viewImportService.AddNuGetPackage(packageName);

            Assert.AreEqual(1, _viewImportService.NewNuGetPackages.Count);
            Assert.True(_viewImportService.NewNuGetPackages.Contains(packageName));
        }

        [Test]
        public void AddNuGetPackage_Does_Not_Add_Duplicates_To_Set()
        {
            var packageName1 = "Fritz.BlazorWebFormsComponents";
            var packageName2 = "Something.Else.Package";

            _viewImportService.AddNuGetPackage(packageName1);
            _viewImportService.AddNuGetPackage(packageName2);
            _viewImportService.AddNuGetPackage(packageName1);

            Assert.AreEqual(2, _viewImportService.NewNuGetPackages.Count);
            Assert.True(_viewImportService.NewNuGetPackages.Contains(packageName1));
            Assert.True(_viewImportService.NewNuGetPackages.Contains(packageName2));
        }
    }
}
