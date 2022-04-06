using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CTA.WebForms.Tests
{
    public class WebFormsTestBase
    {
        public static string AssemblyDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string TestingAreaDir => Path.Combine(AssemblyDir, "TestingArea");
        public static string TestFilesDir => Path.Combine(TestingAreaDir, "TestFiles");
        public static string TestAssembliesDir => Path.Combine(TestFilesDir, "TestAssemblies");

        [OneTimeSetUp]
        public void BaseOneTimeSetup()
        {
            CopyTestTagConfigs();
        }

        private void CopyTestTagConfigs()
        {
            // Project configured to copy TempTagConfigs folder to output directory
            // so no extra action necessary here
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var tempTemplatesDir = Path.Combine(assemblyDir, "TempTagConfigs");
            if (Directory.Exists(tempTemplatesDir))
            {
                var files = Directory.EnumerateFiles(tempTemplatesDir, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(tempTemplatesDir, file);
                    var targetFile = Path.Combine(Rules.Config.Constants.TagConfigsExtractedPath, relativePath);
                    var targetFileDir = Path.GetDirectoryName(targetFile);

                    Directory.CreateDirectory(targetFileDir);
                    File.Copy(file, targetFile, true);
                }
            }
        }

        private protected static bool CreateSimpleCodeBehindCompilation(string content, out ClassDeclarationSyntax classDec, out SemanticModel model)
        {
            var tree = CSharpSyntaxTree.ParseText(content);

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var web = MetadataReference.CreateFromFile(Path.Combine(TestAssembliesDir, "System.Web.dll"));
            var comp = CSharpCompilation.Create("Test", new[] { tree }, new[] { mscorlib, web });

            model = comp.GetSemanticModel(tree);

            if (tree.TryGetRoot(out var root))
            {
                classDec = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
                return true;
            }

            classDec = null;
            return false;
        }
    }
}
