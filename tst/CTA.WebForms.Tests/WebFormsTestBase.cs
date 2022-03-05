using System.IO;
using System.Reflection;

namespace CTA.WebForms.Tests
{
    public static class WebFormsTestBase
    {
        public static string AssemblyDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string TestingAreaDir => Path.Combine(AssemblyDir, "TestingArea");
        public static string TestFilesDir => Path.Combine(TestingAreaDir, "TestFiles");
    }
}
