using System.Linq;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Extensions
{
    public class UstNodeQueriesTests : DetectAllFeaturesTestBase
    {
        [Test]
        public void GetDeclaredClassesByBaseType_Returns_Empty_Collection_If_Declared_Class_With_Base_Type_Is_Not_Found()
        {
            var baseType = "Nonexistent base type";
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.IsEmpty(node.GetDeclaredClassesByBaseType(baseType));
        }

        [Test]
        public void GetInvocationExpressionsBySemanticDefinition_Returns_NonEmpty_Collection_If_InvocationExpression_Is_Found()
        {
            var semanticOriginalDefinition = "System.Web.Mvc.Controller.View()";
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.IsNotEmpty(node.GetInvocationExpressionsBySemanticDefinition(semanticOriginalDefinition));
        }

        [Test]
        public void GetInvocationExpressionsBySemanticDefinition_Returns_Empty_Collection_If_InvocationExpression_Is_Not_Found()
        {
            var semanticOriginalDefinition = "Nonexistent InvocationExpression";
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.IsEmpty(node.GetInvocationExpressionsBySemanticDefinition(semanticOriginalDefinition));
        }

        [Test]
        public void ContainsInvocationExpressionsWithSemanticDefinition_Returns_True_If_InvocationExpression_Is_Found()
        {
            var semanticOriginalDefinition = "System.Web.Mvc.Controller.View()";
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.True(node.ContainsInvocationExpressionsWithSemanticDefinition(semanticOriginalDefinition),
                $"Expected invocation expression in {semanticOriginalDefinition} in {projectName}//{sourceFileName} was not found.");
        }

        [Test]
        public void ContainsInvocationExpressionsWithSemanticDefinition_Returns_False_If_InvocationExpression_Is_Found()
        {
            var semanticOriginalDefinition = "Nonexistent InvocationExpression";
            var projectName = MvcProjectName; 
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.False(node.ContainsInvocationExpressionsWithSemanticDefinition(semanticOriginalDefinition),
                $"Unexpected invocation expression in {semanticOriginalDefinition} in {projectName}//{sourceFileName} was found.");
        }

        [Test]
        public void GetInvocationExpressionsBySemanticReturnType_Returns_InvocationExpressions_With_SemanticReturnType()
        {
            var semanticReturnType = "ViewResult";
            var projectName = MvcProjectName; 
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.True(node.GetInvocationExpressionsBySemanticReturnType(semanticReturnType).Count() == 3);
        }

        [Test]
        public void GetInvocationExpressionsBySemanticReturnType_Returns_InvocationExpressions_With_SemanticReturnTypes()
        {
            var semanticReturnTypes = new[] {"ViewResult"};
            var projectName = MvcProjectName; 
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.True(node.GetInvocationExpressionsBySemanticReturnType(semanticReturnTypes).Count() == 3);
        }

        [Test]
        public void ContainsUsingDirectiveWithIdentifier_Returns_True_If_UsingDirective_Is_Found()
        {
            var identifier = "System.Web.Mvc";
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.True(node.ContainsUsingDirectiveWithIdentifier(identifier),
                $"Expected UsingDirective {identifier} in {projectName}//{sourceFileName} was not found.");
        }

        [Test]
        public void ContainsUsingDirectiveWithIdentifier_Returns_False_If_UsingDirective_Is_Not_Found()
        {
            var identifier = "Nonexistent Namespace";
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.False(node.ContainsUsingDirectiveWithIdentifier(identifier),
                $"Unexpected UsingDirective {identifier} in {projectName}//{sourceFileName} was found.");
        }
    }
}