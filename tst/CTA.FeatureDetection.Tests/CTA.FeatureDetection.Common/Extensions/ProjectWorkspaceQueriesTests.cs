using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;
using System.Linq;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Extensions
{
    public class ProjectWorkspaceQueriesTests : DetectAllFeaturesTestBase
    {
        [Test]
        public void ContainsNugetDependency_Returns_True_If_Nuget_Package_Is_Referenced()
        {
            var nugetReference = "Microsoft.AspNet.WebApi";
            var projectName = WebApiProjectName;
            var projectWorkspace = WebApiAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;

            Assert.True(projectWorkspace.ContainsNugetDependency(nugetReference),
                $"Expected reference to {nugetReference} in {projectName} was not found.");
        }

        [Test]
        public void ContainsNugetDependency_Returns_False_If_Nuget_Package_Is_Not_Referenced()
        {
            var dependency = "Nonexistent dependency";
            var projectName = WebApiProjectName;
            var projectWorkspace = WebApiAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;

            Assert.False(projectWorkspace.ContainsNugetDependency(dependency),
                $"Unexpected reference to {dependency} in {projectName} was found.");
        }

        [Test]
        public void DeclaresClassWithBaseType_Returns_True_If_Class_With_Base_Type_Is_Declared()
        {
            var baseType = "System.Web.Mvc.Controller";
            var projectName = MvcProjectName;
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;

            Assert.True(projectWorkspace.DeclaresClassWithBaseType(baseType),
                $"Expected declaration of class with base type {baseType} in {projectName} was not found.");
        }

        [Test]
        public void GetAllClassDeclarations_Returns_All_Class_Declaration_Nodes_In_Project()
        {
            var projectName = MvcProjectName;
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;

            Assert.AreEqual(5, projectWorkspace.GetAllClassDeclarations().Count());
        }

        [Test]
        public void ContainsNonEmptyDirectory_Returns_True_If_Directory_Is_Found_With_Contents()
        {
            var directoryName = "Views";
            var projectName = MvcProjectName;
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;

            Assert.True(projectWorkspace.ContainsNonEmptyDirectory(directoryName),
                $"Expected {directoryName} in {projectName} to have contents but it was empty.");
        }

        [Test]
        public void ContainsNonEmptyDirectory_Returns_False_If_Directory_Is_Found_Without_Contents()
        {
            var directoryName = "EmptyDirectory";
            var projectName = WebApiProjectName;
            var projectWorkspace = WebApiAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;

            Assert.False(projectWorkspace.DeclaresClassWithBaseType(directoryName),
                $"Expected {directoryName} in {projectName} to be empty but it has contents.");
        }

        [Test]
        public void ContainsNonEmptyDirectory_Returns_False_If_Directory_Is_Not_Found()
        {
            var directoryName = "Nonexistent Directory";
            var projectName = WebApiProjectName;
            var projectWorkspace = WebApiAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;

            Assert.False(projectWorkspace.DeclaresClassWithBaseType(directoryName),
                $"Expected {directoryName} to not be found in {projectName}, but it was.");
        }
    }
}