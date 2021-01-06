using System.Linq;
using Codelyzer.Analysis.Model;
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
        public void GetPublicMethodDeclarations_Returns_Collection_Of_Public_MethodDeclarations()
        {
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName));

            Assert.AreEqual(3, node.GetPublicMethodDeclarations().Count());
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

        [Test]
        public void IsPublic_Returns_True_If_MethodDeclaration_Has_Public_Modifier()
        {
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var methodName = "Index";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName))
                .AllMethods()
                .First(m => m.Identifier.Equals(methodName));

            Assert.True(node.IsPublic(),
                $"Expected {methodName} method in {projectName}//{sourceFileName} to be public.");
        }

        [Test]
        public void HasBaseType_Returns_True_If_ClassDeclaration_Is_Derived_From_BaseType()
        {
            var projectName = MvcProjectName;
            var sourceFileName = "HomeController.cs";
            var className = "HomeController";
            var baseType = "System.Web.Mvc.Controller";
            var projectWorkspace = MvcAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName))
                .AllClasses()
                .First(m => m.Identifier.Equals(className));

            Assert.True(node.HasBaseType(baseType),
                $"Expected {className} class in {projectName}//{sourceFileName} to inherit from {baseType}.");
        }

        [Test]
        public void HasAttribute_Returns_True_If_Node_Has_The_Specified_Attribute()
        {
            var projectName = WebApiProjectName;
            var sourceFileName = "ValuesController.cs";
            var className = "ValuesController";
            var attributeType = "FromBodyAttribute";
            var projectWorkspace = WebApiAnalyzerResults
                .First(r => r.ProjectResult.ProjectName == projectName)
                .ProjectResult;
            var node = projectWorkspace.SourceFileResults
                .First(r => r.FilePath.EndsWith(sourceFileName))
                .AllClasses()
                .First(m => m.Identifier.Equals(className));

            Assert.True(node.HasAttribute(attributeType),
                $"Expected {className} class in {projectName}//{sourceFileName} to have attribute {attributeType}.");
        }
    }
}