using System;
using CTA.Rules.Actions.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using CTA.Rules.Models.Actions.VisualBasic;

namespace CTA.Rules.Test.Actions.VisualBasic
{
    public class ClassActionsTests
    {
        private SyntaxGenerator _syntaxGenerator;
        private TypeBlockActions _typeBlockActions;
        private ClassBlockSyntax _node;
        private MethodBlockSyntax _subNode;
        private MethodBlockSyntax _functionNode;

        private Dictionary<string, TypeBlockSyntax> _blockNodes;

        [SetUp]
        public void SetUp()
        {

            var workspace = new AdhocWorkspace();
            var language = LanguageNames.VisualBasic;
            _syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, language);
            _typeBlockActions = new TypeBlockActions();
            SyntaxTree tree = VisualBasicSyntaxTree.ParseText(@$"
Class MyClass
End Class

Module MyModule
EndModule
");
            _node = tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassBlockSyntax>()
                .FirstOrDefault();
            
            _blockNodes = new Dictionary<string, TypeBlockSyntax>();
            _blockNodes.Add("class", _node);
            _blockNodes.Add("module", tree.GetRoot().DescendantNodes().OfType<ModuleBlockSyntax>().FirstOrDefault());
            
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
        [TestCase("class")]
        [TestCase("module")]
        public void GetAddCommentAction_Adds_Leading_Comment_To_Class_Declaration(string blockType)
        {
            const string commentToAdd = "This is a comment";
            var addCommentFunc = _typeBlockActions.GetAddCommentAction(commentToAdd);
            var newNode = addCommentFunc(_syntaxGenerator, _blockNodes[blockType]);

            var expectedResult = @$"
' Added by CTA: {commentToAdd}";
            Assert.AreEqual(expectedResult, newNode.GetLeadingTrivia().ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void GetChangeNameAction_Changes_Class_Name_To_Specified_Value(string blockType)
        {
            const string newClassName = "NewClassName";
            var changeNameFunc = _typeBlockActions.GetChangeNameAction(newClassName);
            var newNode = changeNameFunc(_syntaxGenerator, _blockNodes[blockType]);
            Assert.AreEqual(newClassName, newNode.BlockStatement.Identifier.ToString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void GetRenameClassAction_Changes_Class_Name_To_Specified_Value(string blockType)
        {
            const string newClassName = "NewClassName";
            var changeNameFunc = _typeBlockActions.GetRenameClassAction(newClassName);
            var newNode = changeNameFunc(_syntaxGenerator, _blockNodes[blockType]);
            Assert.AreEqual(newClassName, newNode.BlockStatement.Identifier.ToString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void GetRemoveAttributeAction_Removes_Specified_Attribute(string blockType)
        {
            const string attributeToRemove = "Serializable";
            var node = _blockNodes[blockType];
            var addAttributeFunc1 = _typeBlockActions.GetAddAttributeAction("Serializable");
            var addAttributeFunc2 = _typeBlockActions.GetAddAttributeAction("SecurityCritical");

            var nodeWithAttributes = addAttributeFunc1(_syntaxGenerator, node);
            nodeWithAttributes = addAttributeFunc2(_syntaxGenerator, nodeWithAttributes);
            
            var removeAttributeFunc = _typeBlockActions.GetRemoveAttributeAction(attributeToRemove);
            var newNode = removeAttributeFunc(_syntaxGenerator, nodeWithAttributes);
            Assert.IsTrue(newNode.BlockStatement.AttributeLists.ToFullString().Contains("<SecurityCritical>"));
            Assert.IsTrue(!newNode.BlockStatement.AttributeLists.ToFullString().Contains("<Serializable"));
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void GetAddAttributeAction_Adds_Attribute(string blockType)
        {
            const string attributeToAdd = "Serializable";
            var addAttributeFunc = _typeBlockActions.GetAddAttributeAction(attributeToAdd);
            var newNode = addAttributeFunc(_syntaxGenerator, _blockNodes[blockType]);
            Assert.AreEqual("<Serializable>", newNode.BlockStatement.AttributeLists.ToString());
        }

        [Test]
        [TestCase(@"Public Sub MySub(i as Integer)
        Console.WriteLine(i)
    End Sub", "class")]
        [TestCase(@"Public Function MyFunction(i as Integer) As Integer
        Console.WriteLine(i)
        Return i
    End Function", "class")]
        [TestCase(@"Public Sub MySub(i as Integer)
        Console.WriteLine(i)
    End Sub", "module")]
        [TestCase(@"Public Function MyFunction(i as Integer) As Integer
        Console.WriteLine(i)
        Return i
    End Function", "module")]
        public void GetAddMethodAction_Adds_Method(string expression, string blockType)
        {
            var addMethodFunc = _typeBlockActions.GetAddMethodAction(expression);
            var newNode = addMethodFunc(_syntaxGenerator, _blockNodes[blockType]);
            Assert.AreEqual(expression, newNode.Members.OfType<MethodBlockSyntax>().FirstOrDefault()?.ToString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void GetRemoveMethodAction_Removes_Specified_Method(string blockType)
        {
            const string methodName = "MyMethod";
            var methodNode = SyntaxFactory.MethodStatement(SyntaxKind.SubStatement,
                SyntaxFactory.Token(SyntaxKind.SubKeyword), methodName);
            var nodeWithMethod = _blockNodes[blockType].AddMembers(methodNode);

            var removeMethodFunc = _typeBlockActions.GetRemoveMethodAction(methodName);
            var newNode = removeMethodFunc(_syntaxGenerator, nodeWithMethod);

            var expectedResult = _blockNodes[blockType].NormalizeWhitespace().ToFullString();
            Assert.AreEqual(expectedResult, newNode.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void RemoveLastBaseClass(string blockType)
        {
            var baseClassname = "ControllerBase";
            var addBaseClass = _typeBlockActions.GetAddBaseClassAction(baseClassname);
            var removeBaseClassMethod = _typeBlockActions.GetRemoveBaseClassAction(baseClassname);

            var nodeWithClass = addBaseClass(_syntaxGenerator, _blockNodes[blockType]);
            nodeWithClass = removeBaseClassMethod(_syntaxGenerator, nodeWithClass);

            //Make sure the inheritance symbol is removed when last base class is removed:
            StringAssert.DoesNotContain(":", nodeWithClass.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void ReplaceMethodModifiers(string blockType)
        {
            const string methodName = "MyMethod";
            var methodNode = SyntaxFactory.MethodStatement(SyntaxKind.SubStatement,
                SyntaxFactory.Token(SyntaxKind.SubKeyword), methodName);
            var nodeWithMethod = _blockNodes[blockType].AddMembers(methodNode);

            var modifier = "Private Async";
            var replaceModifier = _typeBlockActions.GetReplaceMethodModifiersAction(methodName, modifier);

            var node = replaceModifier(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains(modifier, node.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void AddExpression(string blockType)
        {
            string expression = "Dim _next As RequestDelegate";

            var addBaseClass = _typeBlockActions.GetAddExpressionAction(expression);

            var nodeWithExpression = addBaseClass(_syntaxGenerator, _blockNodes[blockType]);

            StringAssert.Contains(expression, nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void AppendConstructorExpression(string blockType)
        {
            var constructorStatements = new SyntaxList<StatementSyntax>();
            constructorStatements = constructorStatements.Add(
                SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("_breadcrumb = breadcrumb")));
            var methodNode = SyntaxFactory.ConstructorBlock(
                SyntaxFactory.SubNewStatement(),
                constructorStatements,
                SyntaxFactory.EndSubStatement());
            var nodeWithMethod = _blockNodes[blockType].AddMembers(methodNode);
            string expression = "_next = [next]";

            var addBaseClass = _typeBlockActions.GetAppendConstructorExpressionAction(expression);

            var nodeWithExpression = addBaseClass(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains(expression, nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void RemoveConstructorBaseInitializer(string blockType)
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

            var nodeWithMethod = _blockNodes[blockType].AddMembers(methodNode);
            string expression = "next";

            var addBaseClass = _typeBlockActions.GetRemoveConstructorInitializerAction(expression);

            var nodeWithExpression = addBaseClass(_syntaxGenerator, nodeWithMethod);

            StringAssert.DoesNotContain(expression, nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void CreateConstructor(string blockType)
        {
            string types = "RequestDelegate, string";
            string identifiers = "[next], value";

            var createConstructorFunc = _typeBlockActions.GetCreateConstructorAction(types: types, identifiers: identifiers);
            var nodeWithExpression = createConstructorFunc(_syntaxGenerator, _blockNodes[blockType]);

            StringAssert.Contains(types.Split(',')[0], nodeWithExpression.ToFullString());
            StringAssert.Contains(identifiers.Split(',')[0], nodeWithExpression.ToFullString());
        }


        [Test]
        [TestCase(true, "class")]
        [TestCase(false, "module")]
        [TestCase(true, "module")]
        [TestCase(false, "class")]
        public void ChangeMethodName(bool isSubMethod, string blockType)
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

            var nodeWithMethod = _blockNodes[blockType].AddMembers(methodNode);

            var changeMethodNameFunc = _typeBlockActions.GetChangeMethodNameAction(existingMethodName, newMethodName);
            var nodeWithExpression = changeMethodNameFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains(newMethodName, nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void RemoveMethodParameters(string blockType)
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
            var nodeWithMethod = _blockNodes[blockType].AddMembers(methodNode);
            var removeMethodParametersFunc = _typeBlockActions.GetRemoveMethodParametersAction(methodName);
            var nodeWithExpression = removeMethodParametersFunc(_syntaxGenerator, nodeWithMethod);
            StringAssert.DoesNotContain("HttpContext", nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void ChangeMethodReturnTaskTypeVoid(string blockType)
        {
            string methodName = "Invoke";
            var nodeWithMethod = _blockNodes[blockType].AddMembers(_subNode);
            var changeMethodToReturnTaskTypeFunc = _typeBlockActions.GetChangeMethodToReturnTaskTypeAction(methodName);
            var nodeWithExpression = changeMethodToReturnTaskTypeFunc(_syntaxGenerator, nodeWithMethod);
            StringAssert.Contains("Task", nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void ChangeMethodReturnTaskTypeString(string blockType)
        {
            string methodName = "TestFunction";
            var nodeWithMethod = _blockNodes[blockType].AddMembers(_functionNode);
            var changeMethodToReturnTaskTypeFunc = _typeBlockActions.GetChangeMethodToReturnTaskTypeAction(methodName);
            var nodeWithExpression = changeMethodToReturnTaskTypeFunc(_syntaxGenerator, nodeWithMethod);
            StringAssert.Contains("Task(Of String)", nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void CommentMethod(string blockType)
        {
            string methodName = "Invoke";
            var nodeWithMethod = _blockNodes[blockType].AddMembers(_subNode);

            var commentMethodeFunc = _typeBlockActions.GetCommentMethodAction(methodName);
            var nodeWithExpression = commentMethodeFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains("' Public Sub Invoke", nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void AddCommentsToMethod(string blockType)
        {
            string methodName = "Invoke";
            string comment = "This method is deprecated";
            var nodeWithMethod = _blockNodes[blockType].AddMembers(_subNode);

            var addCommentsToMethodFunc = _typeBlockActions.GetAddCommentsToMethodAction(methodName, comment);
            var nodeWithExpression = addCommentsToMethodFunc(_syntaxGenerator, nodeWithMethod);

            StringAssert.Contains("' Added by CTA: This method is deprecated", nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void AddExpressionToMethod(string blockType)
        {
            string methodName = "TestFunction";
            string expression = "Await _next.Invoke(context)";
            var nodeWithMethod = _blockNodes[blockType].AddMembers(_functionNode);
            var changeMethodToReturnTaskTypeFunc = _typeBlockActions.GetChangeMethodToReturnTaskTypeAction(methodName);
            var nodeWithExpression = changeMethodToReturnTaskTypeFunc(_syntaxGenerator, nodeWithMethod);

            var addExpressionToMethodFunc = _typeBlockActions.GetAddExpressionToMethodAction(methodName, expression);
            nodeWithExpression = addExpressionToMethodFunc(_syntaxGenerator, nodeWithExpression);

            StringAssert.Contains("Await _next.Invoke(context)", nodeWithExpression.ToFullString());
        }

        [Test]
        [TestCase("class")]
        [TestCase("module")]
        public void AddParametersToMethod(string blockType)
        {
            string methodName = "Invoke";
            string types = "HttpContext,String";
            string identifiers = "context,value";

            var nodeWithMethod = _blockNodes[blockType].AddMembers(_subNode);

            var addParametersToMethodFunc =
                _typeBlockActions.GetAddParametersToMethodAction(methodName, types, identifiers);
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
                ? _typeBlockActions.GetReplaceWebApiControllerMethodsBodyAction(newBody)
                : _typeBlockActions.GetReplaceCoreControllerMethodsBodyAction(newBody);
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
            var classAction = new TypeBlockAction() { Key = "Test", Value = "Test2", TypeBlockActionFunc = _typeBlockActions.GetAddAttributeAction("Test") };
            var cloned = classAction.Clone<TypeBlockAction>();
            Assert.True(classAction.Equals(cloned));
            
            cloned.Value = "DifferentValue";
            Assert.False(classAction.Equals(cloned));
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