using CTA.WebForms.TagCodeBehindHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;

namespace CTA.WebForms.Tests.TagCodeBehindHandlers
{
    [TestFixture]
    public class DefaultTagCodeBehindHandlerTests : WebFormsTestBase
    {
        private DefaultTagCodeBehindHandler _handler;
        private string _idValue;

        [SetUp]
        public void SetUp()
        {
            // We need set up here instead of one time set up because
            // we want to test on a fresh handler every time
            _idValue = "MyLink";
            _handler = new DefaultTagCodeBehindHandler("System.Web.UI.WebControls.HyperLink", _idValue);
        }

        [Test]
        public void StageCodeBehindConversionsForAttribute_Correctly_Stages_Convertable_Nodes()
        {
            var navigateUrl = "NavigateUrl";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, null);

            Assert.AreEqual(1, _handler.StagedConversions.Count());

            var stagedConversion = _handler.StagedConversions.Single();

            Assert.AreEqual($"{_idValue}.{navigateUrl}", stagedConversion.input.ToString());
            Assert.AreEqual($"{_idValue}_{navigateUrl}", stagedConversion.replacement.ToString());
        }

        [Test]
        public void StageCodeBehindConversionsForAttribute_Correctly_Stages_Convertable_Nodes_With_Converted_Source_Value()
        {
            var navigateUrl = "NavigateUrl";
            var convertedSourceValue = "class=\"OtherNewClass\"";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, convertedSourceValue);

            Assert.AreEqual(1, _handler.StagedConversions.Count());

            var stagedConversion = _handler.StagedConversions.Single();

            Assert.AreEqual($"{_idValue}.{navigateUrl}", stagedConversion.input.ToString());
            Assert.AreEqual($"{_idValue}_{navigateUrl}", stagedConversion.replacement.NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void StageCodeBehindConversionsForAttribute_Correctly_Stages_Convertable_Nodes_From_Multiple_Valid_Calls()
        {
            var navigateUrl = "NavigateUrl";
            var cssClass = "CssClass";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
            MyLink.CssClass = ""NewClass"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, null);
            _handler.StageCodeBehindConversionsForAttribute(model, classDec, cssClass, null);

            Assert.AreEqual(2, _handler.StagedConversions.Count());

            var stagedConversion1 = _handler.StagedConversions.First();
            var stagedConversion2 = _handler.StagedConversions.Last();

            Assert.AreEqual($"{_idValue}.{navigateUrl}", stagedConversion1.input.ToString());
            Assert.AreEqual($"{_idValue}_{navigateUrl}", stagedConversion1.replacement.ToString());

            Assert.AreEqual($"{_idValue}.{cssClass}", stagedConversion2.input.ToString());
            Assert.AreEqual($"{_idValue}_{cssClass}", stagedConversion2.replacement.ToString());
        }

        [Test]
        public void StageCodeBehindConversionsForAttribute_Stages_No_Changes_When_No_Convertable_Nodes()
        {
            var navigateUrl = "NavigateUrl";
            var cssClass = "CssClass";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() { }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, null);
            _handler.StageCodeBehindConversionsForAttribute(model, classDec, cssClass, null);

            Assert.AreEqual(0, _handler.StagedConversions.Count());
        }

        [Test]
        public void StageCodeBehindConversionsForAttribute_Does_Nothing_When_Changes_Have_Already_Been_Staged()
        {
            var navigateUrl = "NavigateUrl";
            var convertedSourceValue = "class=\"OtherNewClass\"";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, convertedSourceValue);

            Assert.AreEqual(1, _handler.StagedConversions.Count());

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, convertedSourceValue);

            Assert.AreEqual(1, _handler.StagedConversions.Count());

            var stagedConversion = _handler.StagedConversions.Single();

            Assert.AreEqual($"{_idValue}.{navigateUrl}", stagedConversion.input.ToString());
            Assert.AreEqual($"{_idValue}_{navigateUrl}", stagedConversion.replacement.NormalizeWhitespace().ToFullString());
        }

        [Test]
        public void PerformMemberAdditions_Adds_All_Generated_Properties()
        {
            var navigateUrl = "NavigateUrl";
            var cssClass = "CssClass";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
            MyLink.CssClass = ""NewClass"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, null);
            _handler.StageCodeBehindConversionsForAttribute(model, classDec, cssClass, null);

            classDec = _handler.PerformMemberAdditions(classDec);
            var classString = classDec.NormalizeWhitespace().ToFullString();

            Assert.True(classString.Contains("public String MyLink_NavigateUrl { get; set; }"));
            Assert.True(classString.Contains("public String MyLink_CssClass { get; set; }"));
        }

        [Test]
        public void PerformMemberAdditions_Adds_Generated_Properties_With_Converted_Source_Values()
        {
            var navigateUrl = "NavigateUrl";
            var cssClass = "CssClass";

            var convertedSourceValue = "href=\"https://www.google.com\"";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
            MyLink.CssClass = ""NewClass"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, convertedSourceValue);
            _handler.StageCodeBehindConversionsForAttribute(model, classDec, cssClass, null);

            classDec = _handler.PerformMemberAdditions(classDec);
            var classString = classDec.NormalizeWhitespace().ToFullString();

            Assert.True(classString.Contains(
@$"    // The initial value ""href=""https://www.google.com""""
    // was removed from the view layer in favor
    // of a binding to the following property"));
            Assert.True(classString.Contains("public String MyLink_NavigateUrl { get; set; }"));
            Assert.True(classString.Contains("public String MyLink_CssClass { get; set; }"));
        }

        [Test]
        public void PerformMemberAdditions_Does_Nothing_When_No_Generated_Properties_Exist()
        {
            var navigateUrl = "NavigateUrl";
            var cssClass = "CssClass";

            var convertedSourceValue = "href=\"https://www.google.com\"";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() { }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, convertedSourceValue);
            _handler.StageCodeBehindConversionsForAttribute(model, classDec, cssClass, null);

            var oldClassString = classDec.ToFullString();
            classDec = _handler.PerformMemberAdditions(classDec);
            var classString = classDec.ToFullString();

            Assert.AreEqual(oldClassString, classString);
        }

        [Test]
        public void GetBindingIfExists_Returns_Proper_Binding_With_Target()
        {
            var navigateUrl = "NavigateUrl";
            var target = "href";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, null);
            var result = _handler.GetBindingIfExists(navigateUrl, target);

            Assert.AreEqual($"{target}=\"@({_idValue}_{navigateUrl})\"", result);
        }

        [Test]
        public void GetBindingIfExists_Returns_Proper_Binding_Without_Target()
        {
            var navigateUrl = "NavigateUrl";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            _handler.StageCodeBehindConversionsForAttribute(model, classDec, navigateUrl, null);
            var result = _handler.GetBindingIfExists(navigateUrl, null);

            Assert.AreEqual($"@({_idValue}_{navigateUrl})", result);
        }

        [Test]
        public void GetBindingIfExists_Returns_Null_When_No_Bindable_Property_Exists()
        {
            var navigateUrl = "NavigateUrl";

            string inputClass =
@"using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Test {
    public class TestClass {
        public HyperLink MyLink { get; set; }
        
        public void TestMethod() {
            MyLink.NavigateUrl = ""https://aws.amazon.com"";
        }
    }
}";

            Assert.True(CreateSimpleCodeBehindCompilation(inputClass, out var classDec, out var model));

            var result = _handler.GetBindingIfExists(navigateUrl, null);

            Assert.IsNull(result);
        }
    }
}
