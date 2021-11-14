using CTA.Rules.Actions;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CTA.Rules.Test.Actions
{
    public class ClassActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private ClassActions _classActions;
        private ClassDeclarationSyntax _node;

        [SetUp]
        public void SetUp()
        {

            var workspace = new AdhocWorkspace();
            var language = LanguageNames.CSharp;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _classActions = new ClassActions();
            _node = _syntaxGenerator.ClassDeclaration("MyClass") as ClassDeclarationSyntax;
        }

        [Test]
        public void GetAddCommentAction_Adds_Leading_Comment_To_Class_Declaration()
        {
            const string commentToAdd = "This is a comment";
            var addCommentFunc = _classActions.GetAddCommentAction(commentToAdd);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            var expectedResult = @$"/* Added by CTA: {commentToAdd} */
class MyClass
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetChangeNameAction_Changes_Class_Name_To_Specified_Value()
        {
            const string newClassName = "NewClassName";
            var changeNameFunc = _classActions.GetChangeNameAction(newClassName);
            var newNode = changeNameFunc(_syntaxGenerator, _node);

            var expectedResult = @$"class {newClassName}
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetRenameClassAction_Changes_Class_Name_To_Specified_Value()
        {
            const string newClassName = "NewClassName";
            var changeNameFunc = _classActions.GetRenameClassAction(newClassName);
            var newNode = changeNameFunc(_syntaxGenerator, _node);

            var expectedResult = @$"class {newClassName}
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetRemoveAttributeAction_Removes_Specified_Attribute()
        {
            const string attributeToRemove = "Serializable";
            var nodeWithAttributes = (ClassDeclarationSyntax)_syntaxGenerator.AddAttributes(_node,
                _syntaxGenerator.Attribute("Serializable"),
                _syntaxGenerator.Attribute("SecurityCritical"));

            var removeAttributeFunc = _classActions.GetRemoveAttributeAction(attributeToRemove);
            var newNode = removeAttributeFunc(_syntaxGenerator, nodeWithAttributes);

            var expectedResult = @$"[SecurityCritical]
class MyClass
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetRemoveMethodAction_Removes_Specified_Method()
        {
            const string methodName = "MyMethod";
            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), methodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var removeMethodFunc = _classActions.GetRemoveMethodAction(methodName);
            var newNode = removeMethodFunc(_syntaxGenerator, nodeWithMethod);

            var expectedResult = @$"class MyClass
{{
}}";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void RemoveLastBaseClass()
        {
            var baseClassname = "ControllerBase";
            var addBaseClass = _classActions.GetAddBaseClassAction(baseClassname);
            var removeBaseClassMethod = _classActions.GetRemoveBaseClassAction(baseClassname);

            var nodeWithClass = addBaseClass(_syntaxGenerator, _node);
            nodeWithClass = removeBaseClassMethod(_syntaxGenerator, nodeWithClass);

            //Make sure the inheritance symbol is removed when last base class is removed:
            StringAssert.DoesNotContain(":", nodeWithClass.ToFullString());
        }

        [Test]
        public void ReplaceMethodModifiers()
        {
            const string methodName = "MyMethod";
            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), methodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var modifier = "private async extern";
            var replaceModifier = _classActions.GetReplaceMethodModifiersAction(methodName, modifier);

            var node = replaceModifier(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains(modifier, node.ToFullString());
        }

        [Test]
        public void AddExpression()
        {
            string expression = "RequestDelegate _next = null;";

            var addBaseClass = _classActions.GetAddExpressionAction(expression);

            var nodeWithExpression = addBaseClass(_syntaxGenerator, _node);

            StringAssert.Contains(expression, nodeWithExpression.ToFullString());
        }

        [Test]
        public void AppendConstructorExpression()
        {
            var methodNode = SyntaxFactory.ConstructorDeclaration(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(SyntaxFactory.ParseToken("void")), SyntaxFactory.Identifier("MyClass"), SyntaxFactory.ParameterList(), null, SyntaxFactory.Block(SyntaxFactory.ParseStatement("int i = 5;")), SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            var nodeWithMethod = _node.AddMembers(methodNode);
            string expression = "_next = next;";

            var addBaseClass = _classActions.GetAppendConstructorExpressionAction(expression);

            var nodeWithExpression = addBaseClass(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains(expression, nodeWithExpression.ToFullString());
        }

        [Test]
        public void RemoveConstructorBaseClass()
        {
            List<ParameterSyntax> parameters = new List<ParameterSyntax>
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("breadcrumb")).WithType(SyntaxFactory.ParseTypeName("string"))
            };
            var parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));

            var methodNode = SyntaxFactory.ConstructorDeclaration(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(SyntaxFactory.ParseToken("void")), SyntaxFactory.Identifier("MyClass"), parameterList, null, SyntaxFactory.Block(SyntaxFactory.ParseStatement("_breadcrumb = breadcrumb;")), SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer).AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("next")), SyntaxFactory.Argument(SyntaxFactory.IdentifierName("testing"))));
            var nodeWithMethod = _node.AddMembers(methodNode);
            string expression = "next";

            var addBaseClass = _classActions.GetRemoveConstructorInitializerAction(expression);

            var nodeWithExpression = addBaseClass(_syntaxGenerator, nodeWithMethod);

            StringAssert.DoesNotContain(expression, nodeWithExpression.ToFullString());
        }

        [Test]
        public void CreateConstructor()
        {
            string types = "RequestDelegate, string";
            string identifiers = "next, value";

            var createContructorFunc = _classActions.GetCreateConstructorAction(types: types, identifiers: identifiers);
            var nodeWithExpression = createContructorFunc(_syntaxGenerator, _node);

            StringAssert.Contains(types.Split(',')[0], nodeWithExpression.ToFullString());
            StringAssert.Contains(identifiers.Split(',')[0], nodeWithExpression.ToFullString());
        }


        [Test]
        public void ChangeMethodName()
        {
            string existingMethodName = "ProcessRequest";
            string newMethodName = "Invoke";

            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), existingMethodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var changeMethodNameFunc = _classActions.GetChangeMethodNameAction(existingMethodName, newMethodName);
            var nodeWithExpression = changeMethodNameFunc(_syntaxGenerator, nodeWithMethod);


            StringAssert.Contains(newMethodName, nodeWithExpression.ToFullString());
        }

        [Test]
        public void RemoveMethodParameters()
        {
            List<ParameterSyntax> parameters = new List<ParameterSyntax>
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("context")).WithType(SyntaxFactory.ParseTypeName("HttpContext")),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("value")).WithType(SyntaxFactory.ParseTypeName("string"))
            };
            var parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));

            string methodName = "testMethod";

            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), methodName).WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)));
            var nodeWithMethod = _node.AddMembers(methodNode);

            var removeMethodParametersFunc = _classActions.GetRemoveMethodParametersAction(methodName);
            var nodeWithExpression = removeMethodParametersFunc(_syntaxGenerator, nodeWithMethod);


            StringAssert.DoesNotContain("HttpContext", nodeWithExpression.ToFullString());
        }

        [Test]
        public void ChangeMethodReturnTaskTypeVoid()
        {
            string methodName = "Invoke";
            string returnType = "void";

            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), methodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var ChangeMethodToReturnTaskTypeeFunc = _classActions.GetChangeMethodToReturnTaskTypeAction(methodName);
            var nodeWithExpression = ChangeMethodToReturnTaskTypeeFunc(_syntaxGenerator, nodeWithMethod);


            StringAssert.Contains("Task", nodeWithExpression.ToFullString());
        }

        [Test]
        public void ChangeMethodReturnTaskTypeString()
        {
            string methodName = "Invoke";
            string returnType = "string";

            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), methodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var ChangeMethodToReturnTaskTypeeFunc = _classActions.GetChangeMethodToReturnTaskTypeAction(methodName);
            var nodeWithExpression = ChangeMethodToReturnTaskTypeeFunc(_syntaxGenerator, nodeWithMethod);


            StringAssert.Contains("Task<string>", nodeWithExpression.ToFullString());
        }

        [Test]
        public void CommentMethod()
        {
            string methodName = "Invoke";
            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), methodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var CommentMethodeFunc = _classActions.GetCommentMethodAction(methodName);
            var nodeWithExpression = CommentMethodeFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains("/*voidInvoke()*/", nodeWithExpression.ToFullString());
        }

        [Test]
        public void AddCommentsToMethod()
        {
            string methodName = "Invoke";
            string comment = "This method is deprecated";
            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), methodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var AddCommentsToMethodFunc = _classActions.GetAddCommentsToMethodAction(methodName, comment);
            var nodeWithExpression = AddCommentsToMethodFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains("/* Added by CTA: This method is deprecated */", nodeWithExpression.ToFullString());
        }

        [Test]
        public void AddExpressionToMethod()
        {
            string methodName = "Invoke";
            string expression = "await _next.Invoke(context);";
            var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), methodName).AddBodyStatements();
            var nodeWithMethod = _node.AddMembers(methodNode);

            var AddExpressionToMethodFunc = _classActions.GetAddExpressionToMethodAction(methodName, expression);
            var nodeWithExpression = AddExpressionToMethodFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains("await _next.Invoke(context);", nodeWithExpression.ToFullString());
        }

        [Test]
        public void AddParametersToMethod()
        {
            var methodString = @"void Invoke(){}";
            string methodName = "Invoke";
            string types = "HttpContext, string";
            string identifiers = " context, value";

            var nodeWithMethod = _node.AddMembers(SyntaxFactory.ParseMemberDeclaration(methodString));

            var AddParametersToMethodFunc = _classActions.GetAddParametersToMethodAction(methodName, types, identifiers);
            var nodeWithExpression = AddParametersToMethodFunc(_syntaxGenerator, nodeWithMethod);

            var expectedString = @"classMyClass{void Invoke(HttpContext context, string value){}}";
            StringAssert.Contains(expectedString, nodeWithExpression.ToFullString());
        }

        [Test]
        public void ReplacePublicMethodsBody()
        {
            string newBody = "return Content(MonolithService.CreateRequest());";
            var methodString1 = @"public async Task<string> SuperStringAsyncMethod(){ var hello = ""hello world!""}";
            var methodString2 = @"public string SuperStringMethod(){ var hello = ""hello world again?!""}";
            var methodString3 = @"private string SuperDontTouchMethod(){ var hello = ""Not hello world!""}";
            var nodeWithMethods = _node.AddMembers(SyntaxFactory.ParseMemberDeclaration(methodString1));
            nodeWithMethods = nodeWithMethods.AddMembers(SyntaxFactory.ParseMemberDeclaration(methodString2));
            nodeWithMethods = nodeWithMethods.AddMembers(SyntaxFactory.ParseMemberDeclaration(methodString3));

            var ReplacePublicMethodsBodyFunc = _classActions.GetReplaceMvcControllerMethodsBodyAction(newBody);
            var newNode = ReplacePublicMethodsBodyFunc(_syntaxGenerator, nodeWithMethods);

            var expectedString = @"classMyClass{/* Added by CTA: Modified to call the extracted logic. */
public async Task<ActionResult> SuperStringAsyncMethod()
{
    return Content(await MonolithService.CreateRequestAsync());
}/* Added by CTA: Modified to call the extracted logic. */
public ActionResult SuperStringMethod()
{
    return Content(MonolithService.CreateRequest());
}private string SuperDontTouchMethod(){ var hello = ""Not hello world!""}}";
            StringAssert.Contains(expectedString, newNode.ToFullString());
        }

        [Test]
        public void ClassDeclarationEquals()
        {
            var classAction = new ClassDeclarationAction() { Key = "Test", Value = "Test2", ClassDeclarationActionFunc = _classActions.GetAddAttributeAction("Test") };
            var cloned = classAction.Clone<ClassDeclarationAction>();
            Assert.True(classAction.Equals(cloned));

            cloned.Value = "DifferentValue";
            Assert.False(classAction.Equals(cloned));
        }
    }
}