using CTA.WebForms.Services;
using CTA.WebForms.TagCodeBehindHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CTA.WebForms.Tests.Services
{
    [TestFixture]
    public class CodeBehindReferenceLinkerServiceTests : WebFormsTestBase
    {
        private CodeBehindReferenceLinkerService _service;

        [SetUp]
        public void SetUp()
        {
            // We need set up here instead of one time set up because
            // we want to test on a fresh service every time
            _service = new CodeBehindReferenceLinkerService();
        }

        [Test]
        public async Task HandleCodeBehindForAttribute_Generates_Binding_For_Code_Behind_Attributes_With_Target()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterClassDeclaration(testPath, model, classDec);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            var result = await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);

            Assert.AreEqual("NewTextValue=\"@(MyButton_Text)\"", result);
        }

        [Test]
        public async Task HandleCodeBehindForAttribute_Generates_Binding_For_Code_Behind_Attributes_Without_Target()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterClassDeclaration(testPath, model, classDec);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            var result = await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, null, testHandler, token);

            Assert.AreEqual("@(MyButton_Text)", result);
        }

        [Test]
        public async Task HandleCodeBehindForAttribute_Generates_Bindings_For_Code_Behind_Attributes_Mapped_To_Multiple_Places()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterClassDeclaration(testPath, model, classDec);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            var result1 = await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, null, testHandler, token);
            var result2 = await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);

            Assert.AreEqual("@(MyButton_Text)", result1);
            Assert.AreEqual("NewTextValue=\"@(MyButton_Text)\"", result2);
        }

        [Test]
        public async Task HandleCodeBehindForAttribute_Returns_Null_If_No_Code_Behind_References_Present()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public void TestMethod() {}
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterClassDeclaration(testPath, model, classDec);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            var result = await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);

            Assert.IsNull(result);
        }

        [Test]
        public void HandleCodeBehindForAttribute_Can_Be_Cancelled_Using_Cancellation_Token()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            _service.RegisterViewFile(testPath);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            

            Assert.ThrowsAsync(typeof(TaskCanceledException), async () =>
            {
                var task = _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);
                tokenSource.Cancel();
                await task;
            });
        }

        [Test]
        public void HandleCodeBehindForAttribute_Throws_ArgumentNullException_If_Handler_Is_Null()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterClassDeclaration(testPath, model, classDec);
            _service.RegisterCodeBehindHandler(testPath, testHandler);

            Assert.ThrowsAsync(typeof(ArgumentNullException), async () =>
                await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, null, null, token));
        }

        [Test]
        public void HandleCodeBehindForAttribute_Throws_InvalidOperationException_If_View_File_Not_Registered()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            Assert.ThrowsAsync(typeof(InvalidOperationException), 
                async () => await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token));
        }

        [Test]
        public void RegisterViewFile_Throws_Invalid_Operation_Exception_On_Double_Registration()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            _service.RegisterViewFile(testPath);

            Assert.Throws(typeof(InvalidOperationException), () => _service.RegisterViewFile(testPath));
        }

        [Test]
        public async Task RegisterClassDeclaration_Unblocks_HandleCodeBehindForAttribute()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterCodeBehindHandler(testPath, testHandler);

            var result = _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);

            // Check that not registering class declaration has paused service call
            await Task.Delay(200);
            Assert.False(result.IsCompleted);

            _service.RegisterClassDeclaration(testPath, model, classDec);

            Assert.AreEqual("NewTextValue=\"@(MyButton_Text)\"", await result);
        }

        [Test]
        public async Task RegisterClassDeclaration_On_Different_View_File_Doesnt_Unblock_HandleCodeBehindForAttribute()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var otherPath = Path.Combine("C:", "Something", "Other.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterViewFile(otherPath);
            _service.RegisterCodeBehindHandler(testPath, testHandler);

            var result = _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);

            // Check that not registering class declaration has paused service call
            await Task.Delay(200);
            Assert.False(result.IsCompleted);

            _service.RegisterClassDeclaration(otherPath, model, classDec);

            // Check that incorrect registration has not unblocked service call
            await Task.Delay(200);
            Assert.False(result.IsCompleted);

            _service.RegisterClassDeclaration(testPath, model, classDec);

            Assert.AreEqual("NewTextValue=\"@(MyButton_Text)\"", await result);
        }

        [Test]
        public void RegisterClassDeclaration_Throws_InvalidOperationException_If_View_File_Not_Registered()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            Assert.Throws(typeof(InvalidOperationException), () => _service.RegisterClassDeclaration(testPath, model, classDec));
        }

        [Test]
        public void RegisterClassDeclaration_Throws_ArgumentNullException_When_Semantic_Model_Is_Null()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var _));

            _service.RegisterViewFile(testPath);

            Assert.Throws(typeof(ArgumentNullException), () => _service.RegisterClassDeclaration(testPath, null, classDec));
        }

        [Test]
        public void RegisterClassDeclaration_Throws_ArgumentNullException_When_Class_Declaration_Is_Null()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var _, out var model));

            _service.RegisterViewFile(testPath);

            Assert.Throws(typeof(ArgumentNullException), () => _service.RegisterClassDeclaration(testPath, model, null));
        }

        [Test]
        public async Task NotifyAllHandlerConversionsStaged_Unblocks_ExecuteTagCodeBehindHandlers()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            var resultTask = _service.ExecuteTagCodeBehindHandlersAsync(testPath, model, classDec, token);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);

            // Check that not notifying handlers as executed has paused the service call
            await Task.Delay(200);
            Assert.False(resultTask.IsCompleted);

            _service.NotifyAllHandlerConversionsStaged(testPath);

            var result = (await resultTask).NormalizeWhitespace().ToFullString();

            Assert.True(result.Contains("public String MyButton_Text { get; set; }"));
            Assert.True(result.Contains("MyButton_Text = \"New Text Value\";"));
        }

        [Test]
        public void NotifyAllHandlerConvesionsStaged_Throws_InvalidOperationException_If_View_File_Not_Registered()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");

            Assert.Throws(typeof(InvalidOperationException), () => _service.NotifyAllHandlerConversionsStaged(testPath));
        }

        [Test]
        public async Task ExecuteTagCodeBehindHandlers_Does_Relevant_Code_Behind_Reference_Conversions()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            var resultTask = _service.ExecuteTagCodeBehindHandlersAsync(testPath, model, classDec, token);
            await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);
            _service.NotifyAllHandlerConversionsStaged(testPath);

            var result = (await resultTask).NormalizeWhitespace().ToFullString();

            Assert.True(result.Contains("public String MyButton_Text { get; set; }"));
            Assert.True(result.Contains("MyButton_Text = \"New Text Value\";"));
        }

        [Test]
        public async Task ExecuteTagCodeBehindHandlers_Does_Relevant_Code_Behind_Reference_Conversion_With_Multiple_Handlers()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");

            var buttonHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var otherButtonHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "OtherButton");
            var linkHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.HyperLink",
                "MyLink");

            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        public Button OtherButton { get; set; }
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";

            OtherButton.Text = ""Other new text"";
            OtherButton.CssClass = ""NewButtonClass"";

            MyLink.NavigateUrl = ""https://aws.amazon.com"";
            MyLink.SkinID = null;
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);

            _service.RegisterCodeBehindHandler(testPath, buttonHandler);
            _service.RegisterCodeBehindHandler(testPath, otherButtonHandler);
            _service.RegisterCodeBehindHandler(testPath, linkHandler);

            var resultTask = _service.ExecuteTagCodeBehindHandlersAsync(testPath, model, classDec, token);

            await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, null, buttonHandler, token);
            await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, null, otherButtonHandler, token);
            await _service.HandleCodeBehindForAttributeAsync(testPath, "CssClass", null, null, otherButtonHandler, token);
            await _service.HandleCodeBehindForAttributeAsync(testPath, "NavigateUrl", null, null, linkHandler, token);

            _service.NotifyAllHandlerConversionsStaged(testPath);

            var result = (await resultTask).NormalizeWhitespace().ToFullString();

            Assert.True(result.Contains("public String MyButton_Text { get; set; }"));
            Assert.True(result.Contains("public String OtherButton_Text { get; set; }"));
            Assert.True(result.Contains("public String OtherButton_CssClass { get; set; }"));
            Assert.True(result.Contains("public String MyLink_NavigateUrl { get; set; }"));

            Assert.True(result.Contains("MyButton_Text = \"New Text Value\";"));
            Assert.True(result.Contains("OtherButton_Text = \"Other new text\";"));
            Assert.True(result.Contains("OtherButton_CssClass = \"NewButtonClass\";"));
            Assert.True(result.Contains("MyLink_NavigateUrl = \"https://aws.amazon.com\";"));
            Assert.True(result.Contains("// MyLink.SkinID = null;"));
        }

        [Test]
        public async Task ExecuteTagCodeBehindHandlers_Does_Nothing_If_No_Code_Behind_References()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public void TestMethod() { }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            var resultTask = _service.ExecuteTagCodeBehindHandlersAsync(testPath, model, classDec, token);
            await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);
            _service.NotifyAllHandlerConversionsStaged(testPath);

            var result = (await resultTask).ToFullString();

            Assert.AreEqual(classDec.ToFullString(), result);
        }

        [Test]
        public async Task ExecuteTagCodeBehindHandlers_Does_Nothing_If_View_File_Not_Registered()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var token = new CancellationTokenSource().Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            var resultTask = _service.ExecuteTagCodeBehindHandlersAsync(testPath, model, classDec, token);

            var result = (await resultTask).ToFullString();

            Assert.AreEqual(classDec.ToFullString(), result);
        }

        [Test]
        public async Task ExecuteTagCodeBehindHandlers_Can_Be_Cancelled_Using_Cancellation_Token()
        {
            var testPath = Path.Combine("C:", "Something", "File.aspx");
            var testHandler = new DefaultTagCodeBehindHandler(
                "System.Web.UI.WebControls.Button",
                "MyButton");
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public Button MyButton { get; set; }
        
        public void TestMethod() {
            MyButton.Text = ""New Text Value"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _service.RegisterViewFile(testPath);
            _service.RegisterCodeBehindHandler(testPath, testHandler);
            var resultTask = _service.ExecuteTagCodeBehindHandlersAsync(testPath, model, classDec, token);
            await _service.HandleCodeBehindForAttributeAsync(testPath, "Text", null, "NewTextValue", testHandler, token);

            Assert.ThrowsAsync(typeof(TaskCanceledException), async () =>
            {
                tokenSource.Cancel();
                await resultTask;
            });
        }
    }
}
