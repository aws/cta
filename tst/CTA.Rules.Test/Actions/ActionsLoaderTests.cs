using CTA.Rules.Actions;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;

namespace CTA.Rules.Test.Actions
{
    public class ActionLoaderTests
    {
        ActionsLoader _actionLoader;

        [SetUp]
        public void SetUp()
        {
            _actionLoader = new ActionsLoader(new List<string>());
        }

        [Test]
        public void AttributeActionsTest()
        {
            var changeAttribute = _actionLoader.GetAttributeAction("ChangeAttribute", "attributeName");

            Assert.IsNotNull(changeAttribute);
        }

        [Test]
        public void AttributeListActionsTest()
        {
            var addComment = _actionLoader.GetAttributeListAction("AddComment", "comment");

            Assert.IsNotNull(addComment);
        }

        [Test]
        public void ClassActionsTest()
        {
            var removeBaseClass = _actionLoader.GetClassAction("RemoveBaseClass", "baseClass");
            var addBaseClass = _actionLoader.GetClassAction("AddBaseClass", "baseClass");
            var changeName = _actionLoader.GetClassAction("ChangeName", "className"); 
            var removeAttribute = _actionLoader.GetClassAction("RemoveAttribute", "attributeName");
            var addAttribute = _actionLoader.GetClassAction("AddAttribute", "attribute");
            var addComment = _actionLoader.GetClassAction("AddComment", "{comment: \"comment here\", dontUseCTAPrefix: \"true\"}");
            var addComment2 = _actionLoader.GetClassAction("AddComment", "comment");
            var addMethod = _actionLoader.GetClassAction("AddMethod", "expression");
            var removeMethod = _actionLoader.GetClassAction("RemoveMethod", "methodName");
            var renameClass = _actionLoader.GetClassAction("RenameClass", "newClassName");
            var replaceMethodModifiers = _actionLoader.GetClassAction("ReplaceMethodModifiers", "{ methodName: \"\", modifiers: \"\"}");
            var addExpression = _actionLoader.GetClassAction("AddExpression", "expression"); 
            var removeConstructorInitializer = _actionLoader.GetClassAction("RemoveConstructorInitializer", "baseClass"); 
            var appendConstructorExpression = _actionLoader.GetClassAction("AppendConstructorExpression", "expression");
            var createConstructor = _actionLoader.GetClassAction("CreateConstructor", "{ types: \"type\", identifiers: \"identify\"}");
            var changeMethodName = _actionLoader.GetClassAction("ChangeMethodName", "{ existingMethodName: \"method\", newMethodName: \"newMethod\"}");
            var changeMethodToReturnTaskType = _actionLoader.GetClassAction("ChangeMethodToReturnTaskType", "methodName");
            var removeMethodParameters = _actionLoader.GetClassAction("RemoveMethodParameters", "methodName");
            var commentMethod = _actionLoader.GetClassAction("CommentMethod", "{ methodName: \"SuperMethod\", comment: \"comment here\", dontUseCTAPrefix: \"true\"}");
            var addCommentsToMethod = _actionLoader.GetClassAction("AddCommentsToMethod", "{ methodName: \"SuperMethod\", comment: \"comments here\", dontUseCTAPrefix: \"false\"}");
            var addCommentsToMethod2 = _actionLoader.GetClassAction("AddCommentsToMethod", "{ methodName: \"SuperMethod\", comment: \"comments here\" }");
            var addExpressionToMethod = _actionLoader.GetClassAction("AddExpressionToMethod", "{ methodName: \"DuperMethod\", expression: \"var maths = 1+1;\"}");
            var addParametersToMethod = _actionLoader.GetClassAction("AddParametersToMethod", "{ methodName: \"MethodHere\", types: \"type\", identifiers: \"identifier\"}");
            var replaceMvcControllerMethodsBody = _actionLoader.GetClassAction("ReplaceMvcControllerMethodsBody", "expression");
            var replaceWebApiControllerMethodsBody = _actionLoader.GetClassAction("ReplaceWebApiControllerMethodsBody", "expression");

            Assert.IsNotNull(removeBaseClass);
            Assert.IsNotNull(addBaseClass);
            Assert.IsNotNull(changeName);
            Assert.IsNotNull(removeAttribute);
            Assert.IsNotNull(addAttribute);
            Assert.IsNotNull(addComment);
            Assert.IsNotNull(addComment2);
            Assert.IsNotNull(addMethod);
            Assert.IsNotNull(removeMethod);
            Assert.IsNotNull(renameClass);
            Assert.IsNotNull(replaceMethodModifiers);
            Assert.IsNotNull(addExpression);
            Assert.IsNotNull(removeConstructorInitializer);
            Assert.IsNotNull(appendConstructorExpression);
            Assert.IsNotNull(createConstructor);
            Assert.IsNotNull(changeMethodName);
            Assert.IsNotNull(changeMethodToReturnTaskType);
            Assert.IsNotNull(removeMethodParameters);
            Assert.IsNotNull(commentMethod);
            Assert.IsNotNull(addCommentsToMethod);
            Assert.IsNotNull(addCommentsToMethod2);
            Assert.IsNotNull(addExpressionToMethod);
            Assert.IsNotNull(addParametersToMethod);
            Assert.IsNotNull(replaceMvcControllerMethodsBody);
            Assert.IsNotNull(replaceWebApiControllerMethodsBody);
        }

        [Test]
        public void CompilationUnitActionsTest()
        {
            var addDirective = _actionLoader.GetCompilationUnitAction("AddDirective", "namespace");
            var removedirective = _actionLoader.GetCompilationUnitAction("RemoveDirective", "namespace");
            var addComment = _actionLoader.GetCompilationUnitAction("AddComment", "comment");

            Assert.IsNotNull(addDirective);
            Assert.IsNotNull(removedirective);
            Assert.IsNotNull(addComment);
        }

        [Test]
        public void ElementAccessActionsTest()
        {
            var replaceElementAccess = _actionLoader.GetElementAccessExpressionActions("ReplaceElementAccess", "newExpression");
            var addComment = _actionLoader.GetElementAccessExpressionActions("AddComment", "comment");

            Assert.IsNotNull(replaceElementAccess);
            Assert.IsNotNull(addComment);
        }

        [Test]
        public void ExpressionActionsTest()
        {
            var addAwaitOperator = _actionLoader.GetExpressionAction("AddAwaitOperator", "string");
            var addComment = _actionLoader.GetExpressionAction("AddComment", "comment");

            Assert.IsNotNull(addAwaitOperator);
            Assert.IsNotNull(addComment);
        }

        [Test]
        public void IdentifierNameActionsTest()
        {
            var replaceIdentifier = _actionLoader.GetIdentifierNameAction("ReplaceIdentifier", "identifierName");
            var replaceIdentifierInsideClass = _actionLoader.GetIdentifierNameAction("ReplaceIdentifierInsideClass", "{ Identifier: \"identifier\", ClassFullKey: \"classKey\" }");

            Assert.IsNotNull(replaceIdentifier);
            Assert.IsNotNull(replaceIdentifierInsideClass);
        }

        [Test]
        public void InterfaceActionsTest()
        {
            var changeName = _actionLoader.GetInterfaceAction("ChangeName", "newName");
            var removeAttribute = _actionLoader.GetInterfaceAction("RemoveAttribute", "attributeName");
            var addAttribute = _actionLoader.GetInterfaceAction("AddAttribute", "attribute");
            var addComment = _actionLoader.GetInterfaceAction("AddComment", "comment");
            var addMethod = _actionLoader.GetInterfaceAction("AddMethod", "expression");
            var removeMethod = _actionLoader.GetInterfaceAction("RemoveMethod", "methodName");

            Assert.IsNotNull(changeName);
            Assert.IsNotNull(addComment);
            Assert.IsNotNull(removeAttribute);
            Assert.IsNotNull(addAttribute);
            Assert.IsNotNull(addMethod);
            Assert.IsNotNull(removeMethod);
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
        public void MemberAccessActionsTest()
        {
            var addComment = _actionLoader.GetMemberAccessExpressionActions("AddComment", "comment");
            var removeMemberAccess = _actionLoader.GetMemberAccessExpressionActions("RemoveMemberAccess", "string");

            Assert.IsNotNull(addComment);
            Assert.IsNotNull(removeMemberAccess);
        }

        [Test]
        public void MethodDeclarationActionsTest()
        {
            var addComment = _actionLoader.GetClassAction("AddComment", "{comment: \"comment here\", dontUseCTAPrefix: \"true\"}");
            var addComment2 = _actionLoader.GetClassAction("AddComment", "comment");
            var appendExpression = _actionLoader.GetMethodDeclarationAction("AppendExpression", "expression");
            var changeMethodName = _actionLoader.GetMethodDeclarationAction("ChangeMethodName", "newMethodName");
            var changeMethodToReturnTaskType = _actionLoader.GetMethodDeclarationAction("ChangeMethodToReturnTaskType", "newMethodName");
            var removeMethodParameters = _actionLoader.GetMethodDeclarationAction("RemoveMethodParameters", null);
            var addParametersToMethod = _actionLoader.GetMethodDeclarationAction("AddParametersToMethod", "{ types: \"type\", identifiers: \"identifiers\" }");
            var commentMethod = _actionLoader.GetMethodDeclarationAction("CommentMethod", "{ comment: \"comment\", dontUseCTAPrefix: \"true\" }");
            var commentMethod2 = _actionLoader.GetMethodDeclarationAction("CommentMethod", "{ comment: \"comment\" }");
            var addExpressionToMethod = _actionLoader.GetMethodDeclarationAction("AddExpressionToMethod", "expression");

            Assert.IsNotNull(addComment);
            Assert.IsNotNull(addComment2);
            Assert.IsNotNull(appendExpression);
            Assert.IsNotNull(changeMethodName);
            Assert.IsNotNull(changeMethodToReturnTaskType);
            Assert.IsNotNull(removeMethodParameters);
            Assert.IsNotNull(addParametersToMethod);
            Assert.IsNotNull(commentMethod);
            Assert.IsNotNull(commentMethod2);
            Assert.IsNotNull(addExpressionToMethod);
        }

        [Test]
        public void NamespaceActionsTest()
        {
            var renameNamespace = _actionLoader.GetNamespaceActions("RenameNamespace", "newName");

            Assert.IsNotNull(renameNamespace);
        }

        [Test]
        public void ObjectCreationExpressionActionsTest()
        {
            var replaceObjectinitialization = _actionLoader.GetObjectCreationExpressionActions("ReplaceObjectinitialization", "newStatement");
            var replaceObjectWithInvocation = _actionLoader.GetObjectCreationExpressionActions("ReplaceObjectWithInvocation", "{ NewExpression: \"expression\", UseParameters: \"params\" }");
            var replaceOrAddObjectPropertyIdentifier = _actionLoader.GetObjectCreationExpressionActions("ReplaceOrAddObjectPropertyIdentifier", "{ oldMember: \"old\", newMember: \"new\", newValueIfAdding: \"value\" }");
            var replaceOrAddObjectPropertyIdentifier2 = _actionLoader.GetObjectCreationExpressionActions("ReplaceOrAddObjectPropertyIdentifier", "{ oldMember: \"old\", newMember: \"new\" }");
            var replaceObjectPropertyValue = _actionLoader.GetObjectCreationExpressionActions("ReplaceObjectPropertyValue", "{ oldMember: \"old\", newMember: \"new\" }");
            var addComment = _actionLoader.GetObjectCreationExpressionActions("AddComment", "comment");

            Assert.IsNotNull(replaceObjectinitialization);
            Assert.IsNotNull(replaceObjectWithInvocation);
            Assert.IsNotNull(replaceOrAddObjectPropertyIdentifier);
            Assert.IsNotNull(replaceOrAddObjectPropertyIdentifier2);
            Assert.IsNotNull(replaceObjectPropertyValue);
            Assert.IsNotNull(addComment);
        }

        [Test]
        public void ProjectFileActionsTest()
        {
            var migrateProjectFile = _actionLoader.GetProjectFileActions("MigrateProjectFile", "string");

            Assert.IsNotNull(migrateProjectFile);
        }

        [Test]
        public void ProjectLevelActionsTest()
        {
            var archiveFiles = _actionLoader.GetProjectLevelActions("ArchiveFiles", "archiveFiles");
            var CreateNet3FolderHierarchy = _actionLoader.GetProjectLevelActions("CreateNet3FolderHierarchy", "string");
            var CreateNet5FolderHierarchy = _actionLoader.GetProjectLevelActions("CreateNet5FolderHierarchy", "string");
            var CreateNet6FolderHierarchy = _actionLoader.GetProjectLevelActions("CreateNet6FolderHierarchy", "string");
            var MigrateConfig = _actionLoader.GetProjectLevelActions("MigrateConfig", "string");
            var CreateMonolithService = _actionLoader.GetProjectLevelActions("CreateMonolithService", "{ namespaceString: \"namespace\", projectName: \"project\" }");

            Assert.IsNotNull(archiveFiles);
            Assert.IsNotNull(CreateNet3FolderHierarchy);
            Assert.IsNotNull(CreateNet5FolderHierarchy);
            Assert.IsNotNull(CreateNet6FolderHierarchy);
            Assert.IsNotNull(MigrateConfig);
            Assert.IsNotNull(CreateMonolithService);
        }
    }
}