using CTA.Rules.Actions;
using NUnit.Framework;
using System.Collections.Generic;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class ActionLoaderTests
    {
        private VisualBasicActionsLoader _actionLoader;

        [SetUp]
        public void SetUp()
        {
            _actionLoader = new VisualBasicActionsLoader(new List<string>());
        }

        [Test]
        public void CompilationUnitActionsTest()
        {
            var addStatement = _actionLoader.GetCompilationUnitAction("AddStatement", "namespace");
            var removeStatement = _actionLoader.GetCompilationUnitAction("RemoveStatement", "namespace");
            var addComment = _actionLoader.GetCompilationUnitAction("AddComment", "comment");

            Assert.IsNotNull(addStatement);
            Assert.IsNotNull(removeStatement);
            Assert.IsNotNull(addComment);
        }

        [Test]
        public void InvocationExpressionActionsTest()
        {
            var replaceMethodWithObjectAndParameters = _actionLoader.GetInvocationExpressionAction("ReplaceMethodWithObjectAndParameters", "{newMethod: \"method\", newParameters: \"params\"}");
            var replaceMethodWithObject = _actionLoader.GetInvocationExpressionAction("ReplaceMethodWithObject", "newMethod");
            var replaceMethodWithObjectAddType = _actionLoader.GetInvocationExpressionAction("ReplaceMethodWithObjectAddType", "newMethod");
            var replaceMethodAndParameters = _actionLoader.GetInvocationExpressionAction("ReplaceMethodAndParameters", "{ oldMethod: \"method\", newMethod: \"method\", newParameters: \"params\"}");
            var replaceMethodOnly = _actionLoader.GetInvocationExpressionAction("ReplaceMethodOnly", "{ oldMethod: \"method\", newMethod: \"method\" }");
            var replaceParametersOnly = _actionLoader.GetInvocationExpressionAction("ReplaceParametersOnly", "newParameters");
            var appendMethod = _actionLoader.GetInvocationExpressionAction("AppendMethod", "appendMethod");
            var addComment = _actionLoader.GetInvocationExpressionAction("AddComment", "comment");

            Assert.IsNotNull(replaceMethodWithObjectAndParameters);
            Assert.IsNotNull(replaceMethodWithObject);
            Assert.IsNotNull(replaceMethodWithObjectAddType);
            Assert.IsNotNull(replaceMethodAndParameters);
            Assert.IsNotNull(replaceMethodOnly);
            Assert.IsNotNull(replaceParametersOnly);
            Assert.IsNotNull(appendMethod);
            Assert.IsNotNull(addComment);
        }
        
        [Test]
        public void NamespaceActionsTest()
        {
            var renameNamespace = _actionLoader.GetNamespaceActions("RenameNamespace", "newName");

            Assert.IsNotNull(renameNamespace);
        }
    }
}