using System;
using CTA.Rules.Actions.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class ClassActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private ClassActions _classActions;
        private ClassBlockSyntax _node;
        private MethodBlockSyntax _subNode;
        private MethodBlockSyntax _functionNode;

        [SetUp]
        public void SetUp()
        {

            var workspace = new AdhocWorkspace();
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _classActions = new ClassActions();
            SyntaxTree tree = VisualBasicSyntaxTree.ParseText(@$"Class MyClass
End Class");
            _node = tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassBlockSyntax>()
                .FirstOrDefault();

            _subNode = CreateMethodNode("Invoke",
                new List<StatementSyntax>()
                {
                    SyntaxFactory.ParseExecutableStatement(@"' Nothing to see here")
                },
                true,
                new List<SyntaxKind>()
                {
                    SyntaxKind.PublicKeyword
                });

            _functionNode = CreateMethodNode("TestFunction", 
                new List<StatementSyntax>()
                {
                    SyntaxFactory.ParseExecutableStatement(@"' Nothing to see here"),
                    SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression("\"hello world\""))
                }, false,
                new List<SyntaxKind>()
                {
                    SyntaxKind.PublicKeyword
                },
                "String");
        }

        [Test]
        public void GetAddCommentAction_Adds_Leading_Comment_To_Class_Declaration()
        {
            const string commentToAdd = "This is a comment";
            var addCommentFunc = _classActions.GetAddCommentAction(commentToAdd);
            var newNode = addCommentFunc(_syntaxGenerator, _node);

            var expectedResult = @$"' Added by CTA: {commentToAdd}
Class MyClass
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetChangeNameAction_Changes_Class_Name_To_Specified_Value()
        {
            const string newClassName = "NewClassName";
            var changeNameFunc = _classActions.GetChangeNameAction(newClassName);
            var newNode = changeNameFunc(_syntaxGenerator, _node);

            var expectedResult = @$"Class {newClassName}
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetRenameClassAction_Changes_Class_Name_To_Specified_Value()
        {
            const string newClassName = "NewClassName";
            var changeNameFunc = _classActions.GetRenameClassAction(newClassName);
            var newNode = changeNameFunc(_syntaxGenerator, _node);

            var expectedResult = @$"Class {newClassName}
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetRemoveAttributeAction_Removes_Specified_Attribute()
        {
            const string attributeToRemove = "Serializable";
            var nodeWithAttributes = (ClassBlockSyntax)_syntaxGenerator.AddAttributes(_node,
                _syntaxGenerator.Attribute("Serializable"),
                _syntaxGenerator.Attribute("SecurityCritical"));

            var removeAttributeFunc = _classActions.GetRemoveAttributeAction(attributeToRemove);
            var newNode = removeAttributeFunc(_syntaxGenerator, nodeWithAttributes);

            var expectedResult = @$"<SecurityCritical>
Class MyClass
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetAddAttributeAction_Adds_Attribute()
        {
            const string attributeToAdd = "Serializable";
            var addAttributeFunc = _classActions.GetAddAttributeAction(attributeToAdd);
            var newNode = addAttributeFunc(_syntaxGenerator, _node);
            var expectedResult = @$"<Serializable>
Class MyClass
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        [TestCase("\r\n    Public Sub MySub(i as Integer) Console  .WriteLine(i) End Sub ")]
        [TestCase("\r\n    Public Function MyFunction(i as Integer) Console  .WriteLine(i)  : Return i End  Function ")]
        public void GetAddMethodAction_Adds_Method(string expression)
        {
            var addMethodFunc = _classActions.GetAddMethodAction(expression);

            var newNode = addMethodFunc(_syntaxGenerator, _node);

            var expectedResult = @$"Class MyClass
{expression}
End Class";
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        public void GetRemoveMethodAction_Removes_Specified_Method()
        {
            const string methodName = "MyMethod";
            var methodNode = SyntaxFactory.MethodStatement(SyntaxKind.SubStatement,
                SyntaxFactory.Token(SyntaxKind.SubKeyword), methodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var removeMethodFunc = _classActions.GetRemoveMethodAction(methodName);
            var newNode = removeMethodFunc(_syntaxGenerator, nodeWithMethod);

            var expectedResult = @$"Class MyClass
End Class";
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
            var methodNode = SyntaxFactory.MethodStatement(SyntaxKind.SubStatement,
                SyntaxFactory.Token(SyntaxKind.SubKeyword), methodName);
            var nodeWithMethod = _node.AddMembers(methodNode);

            var modifier = "private async extern";
            var replaceModifier = _classActions.GetReplaceMethodModifiersAction(methodName, modifier);

            var node = replaceModifier(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains(modifier, node.ToFullString());
        }

        [Test]
        public void AddExpression()
        {
            string expression = "Dim _next As RequestDelegate";

            var addBaseClass = _classActions.GetAddExpressionAction(expression);

            var nodeWithExpression = addBaseClass(_syntaxGenerator, _node);

            StringAssert.Contains(expression, nodeWithExpression.ToFullString());
        }

        [Test]
        public void AppendConstructorExpression()
        {
            var constructorStatements = new SyntaxList<StatementSyntax>();
            constructorStatements = constructorStatements.Add(
                SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("_breadcrumb = breadcrumb")));
            var methodNode = SyntaxFactory.ConstructorBlock(
                SyntaxFactory.SubNewStatement(),
                constructorStatements,
                SyntaxFactory.EndSubStatement());
            var nodeWithMethod = _node.AddMembers(methodNode);
            string expression = "_next = [next]";

            var addBaseClass = _classActions.GetAppendConstructorExpressionAction(expression);

            var nodeWithExpression = addBaseClass(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains(expression, nodeWithExpression.ToFullString());
        }

        [Test]
        public void RemoveConstructorBaseInitializer()
        {
            var constructorStatements = new SyntaxList<StatementSyntax>();
            constructorStatements = constructorStatements.Add(
                SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("MyBase.New(next, testing)")));
            constructorStatements = constructorStatements.Add(
                SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("_breadcrumb = breadcrumb")));

            var methodNode = SyntaxFactory.ConstructorBlock(
                SyntaxFactory.SubNewStatement(),
                constructorStatements,
                SyntaxFactory.EndSubStatement());

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
            string identifiers = "[next], value";

            var createConstructorFunc = _classActions.GetCreateConstructorAction(types: types, identifiers: identifiers);
            var nodeWithExpression = createConstructorFunc(_syntaxGenerator, _node);

            StringAssert.Contains(types.Split(',')[0], nodeWithExpression.ToFullString());
            StringAssert.Contains(identifiers.Split(',')[0], nodeWithExpression.ToFullString());
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ChangeMethodName(bool isSubMethod)
        {
            string existingMethodName = "ProcessRequest";
            string newMethodName = "Invoke";

            MethodBlockSyntax methodNode;
            if (isSubMethod)
            {
                methodNode = SyntaxFactory.MethodBlock(SyntaxKind.SubBlock,
                    SyntaxFactory.MethodStatement(SyntaxKind.SubStatement, SyntaxFactory.Token(SyntaxKind.SubKeyword),
                        existingMethodName),
                    SyntaxFactory.EndSubStatement());
            }
            else
            {
                methodNode = SyntaxFactory.MethodBlock(SyntaxKind.FunctionBlock,
                    SyntaxFactory.MethodStatement(SyntaxKind.FunctionStatement,
                        SyntaxFactory.Token(SyntaxKind.FunctionKeyword),
                        existingMethodName),
                    SyntaxFactory.EndFunctionStatement());
            }

            var nodeWithMethod = _node.AddMembers(methodNode);

            var changeMethodNameFunc = _classActions.GetChangeMethodNameAction(existingMethodName, newMethodName);
            var nodeWithExpression = changeMethodNameFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains(newMethodName, nodeWithExpression.ToFullString());
        }

        [Test]
        public void RemoveMethodParameters()
        {
            var parameters = new List<ParameterSyntax>
            {
                SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier("context"))
                    .WithAsClause(SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName("HttpContext"))),
                SyntaxFactory.Parameter(SyntaxFactory.ModifiedIdentifier("value"))
                    .WithAsClause(SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName("string")))
            };
            var methodName = "Invoke";
            var methodNode = _subNode.WithSubOrFunctionStatement(
                _subNode.SubOrFunctionStatement.WithParameterList(
                    SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters))));
            var nodeWithMethod = _node.AddMembers(methodNode);
            var removeMethodParametersFunc = _classActions.GetRemoveMethodParametersAction(methodName);
            var nodeWithExpression = removeMethodParametersFunc(_syntaxGenerator, nodeWithMethod);
            StringAssert.DoesNotContain("HttpContext", nodeWithExpression.ToFullString());
        }

        [Test]
        public void ChangeMethodReturnTaskTypeVoid()
        {
            string methodName = "Invoke";
            var nodeWithMethod = _node.AddMembers(_subNode);
            var changeMethodToReturnTaskTypeFunc = _classActions.GetChangeMethodToReturnTaskTypeAction(methodName);
            var nodeWithExpression = changeMethodToReturnTaskTypeFunc(_syntaxGenerator, nodeWithMethod);
            StringAssert.Contains("Task", nodeWithExpression.ToFullString());
        }

        [Test]
        public void ChangeMethodReturnTaskTypeString()
        {
            string methodName = "TestFunction";
            var nodeWithMethod = _node.AddMembers(_functionNode);
            var changeMethodToReturnTaskTypeFunc = _classActions.GetChangeMethodToReturnTaskTypeAction(methodName);
            var nodeWithExpression = changeMethodToReturnTaskTypeFunc(_syntaxGenerator, nodeWithMethod);
            StringAssert.Contains("Task(Of String)", nodeWithExpression.ToFullString());
        }

        [Test]
        public void CommentMethod()
        {
            string methodName = "Invoke";
            var nodeWithMethod = _node.AddMembers(_subNode);

            var commentMethodeFunc = _classActions.GetCommentMethodAction(methodName);
            var nodeWithExpression = commentMethodeFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains("' Public Sub Invoke", nodeWithExpression.ToFullString());
        }

        [Test]
        public void AddCommentsToMethod()
        {
            string methodName = "Invoke";
            string comment = "This method is deprecated";
            var nodeWithMethod = _node.AddMembers(_subNode);

            var addCommentsToMethodFunc = _classActions.GetAddCommentsToMethodAction(methodName, comment);
            var nodeWithExpression = addCommentsToMethodFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains("' Added by CTA: This method is deprecated", nodeWithExpression.ToFullString());
        }

        [Test]
        public void AddExpressionToMethod()
        {
            string methodName = "TestFunction";
            string expression = "Await _next.Invoke(context)";
            var nodeWithMethod = _node.AddMembers(_functionNode);
            var changeMethodToReturnTaskTypeFunc = _classActions.GetChangeMethodToReturnTaskTypeAction(methodName);
            var nodeWithExpression = changeMethodToReturnTaskTypeFunc(_syntaxGenerator, nodeWithMethod);

            var addExpressionToMethodFunc = _classActions.GetAddExpressionToMethodAction(methodName, expression);
            nodeWithExpression = addExpressionToMethodFunc(_syntaxGenerator, nodeWithExpression);

            StringAssert.Contains("Await _next.Invoke(context)", nodeWithExpression.ToFullString());
        }

        [Test]
        public void AddParametersToMethod()
        {
            string methodName = "Invoke";
            string types = "HttpContext,String";
            string identifiers = "context,value";

            var nodeWithMethod = _node.AddMembers(_subNode);

            var addParametersToMethodFunc =
                _classActions.GetAddParametersToMethodAction(methodName, types, identifiers);
            var nodeWithExpression = addParametersToMethodFunc(_syntaxGenerator, nodeWithMethod);

            var expectedString = @"Public Sub Invoke(context As HttpContext, value As String)";
            StringAssert.Contains(expectedString, nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("WebApiController")]
        [TestCase("CoreController")]
        public void ReplacePublicMethodsBody(string controller)
        {
            string newBody = "Return Content(MonolithService.CreateRequest())";
            var nodeWithMethods = _node.AddMembers(CreateMethodNode("SuperStringAsyncMethod",
                new List<StatementSyntax>()
                {
                    SyntaxFactory.ParseExecutableStatement(@"Dim hello = ""hello world""")
                },
                false, new List<SyntaxKind>() { SyntaxKind.PublicKeyword, SyntaxKind.AsyncKeyword },
                "Task(Of String)"));
            nodeWithMethods = nodeWithMethods.AddMembers(CreateMethodNode("SuperStringMethod",
                new List<StatementSyntax>()
                {
                    SyntaxFactory.ParseExecutableStatement(@"Dim hello = ""hello world again?!""")
                },
                false, new List<SyntaxKind>() { SyntaxKind.PublicKeyword },
                "String"));
            nodeWithMethods = nodeWithMethods.AddMembers(CreateMethodNode("SuperStringAsyncMethod",
                new List<StatementSyntax>()
                {
                    SyntaxFactory.ParseExecutableStatement(@"Dim hello = ""not hello world!"""),
                },
                false, new List<SyntaxKind>() { SyntaxKind.PrivateKeyword},
                "String"));

            var replacePublicMethodsBodyFunc = controller == "WebApiController"
                ? _classActions.GetReplaceWebApiControllerMethodsBodyAction(newBody)
                : _classActions.GetReplaceCoreControllerMethodsBodyAction(newBody);
            var newNode = replacePublicMethodsBodyFunc(_syntaxGenerator, nodeWithMethods.NormalizeWhitespace());

            var publicMembers = newNode.Members.OfType<MethodStatementSyntax>().Where(m =>
                m.Modifiers.Any(modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)));
            var privateMembers = newNode.Members.OfType<MethodStatementSyntax>().Where(m =>
                m.Modifiers.Any(modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));

            Assert.IsTrue(publicMembers.All(m => m.ToFullString().Contains($"'Added by CTA: Replace method body with {newBody}")));
            Assert.IsTrue(privateMembers.All(m => !m.ToFullString().Contains($"'Added by CTA: Replace method body with {newBody}")));
        }

        [Test]
        public void ClassDeclarationEquals()
        {
            throw new NotImplementedException();
            // var classAction = new ClassDeclarationAction() { Key = "Test", Value = "Test2", ClassDeclarationActionFunc = _classActions.GetAddAttributeAction("Test") };
            // var cloned = classAction.Clone<ClassDeclarationAction>();
            // Assert.True(classAction.Equals(cloned));
            //
            // cloned.Value = "DifferentValue";
            // Assert.False(classAction.Equals(cloned));
        }

        private MethodBlockSyntax CreateMethodNode(string identifier,
            List<StatementSyntax> bodyExpressions,
            bool isSub,
            List<SyntaxKind> modifiers,
            string returnType = "")
        {
            var body = new SyntaxList<StatementSyntax>(bodyExpressions);
            var modifiersSyntax = new SyntaxTokenList(modifiers.Select(m => SyntaxFactory.Token(m)));
            var blockKind = isSub ? SyntaxKind.SubBlock: SyntaxKind.FunctionBlock;
            var statementKind = isSub ? SyntaxKind.SubStatement : SyntaxKind.FunctionStatement;
            var keyword = isSub? SyntaxKind.SubKeyword : SyntaxKind.FunctionKeyword;
            var endStatement = isSub ? SyntaxFactory.EndSubStatement() : SyntaxFactory.EndFunctionStatement();
            
            return SyntaxFactory.MethodBlock(blockKind,
                SyntaxFactory.MethodStatement(statementKind,
                        SyntaxFactory.Token(keyword), identifier)
                    .WithModifiers(modifiersSyntax)
                    .WithAsClause(SyntaxFactory.SimpleAsClause(SyntaxFactory.ParseTypeName(returnType))),
                endStatement).WithStatements(body).NormalizeWhitespace();
        }
    }
}