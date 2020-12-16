using System;
using System.IO;
using System.Reflection;

namespace CTA.FeatureDetection.Tests.Utils
{
    public class TestUtils
    {
        public static string GetTstPathFromType(Type type)
        {
            var projectPath = Path.GetDirectoryName(type.Assembly.Location);
            var tstPath = Path.GetFullPath(Path.Combine(projectPath, "..", "..", "..", ".."));
            return tstPath;
        }

        public static string GetSrcPath(Type type)
        {
            var projectPath = Path.GetDirectoryName(type.Assembly.Location);
            var srcPath = Path.GetFullPath(Path.Combine(projectPath, "..", "..", "..", "..", "..", "src"));
            return srcPath;
        }

        public static string GetTstPath()
        {
            return GetTstPathFromType(typeof(TestUtils));
        }

        public static string GetTestAssemblySourceDirectory(Type type)
        {
            var tstPath = GetTstPathFromType(type);
            var assembly = Assembly.GetAssembly(type);
            var assemblyProjectName = assembly.GetName().Name;

            return Path.Combine(tstPath, assemblyProjectName);
        }

        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            var files = source.GetFiles();
            foreach (var file in files)
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }

            var dirs = source.GetDirectories();
            foreach (var dir in dirs)
            {
                DirectoryInfo destinationSub = new DirectoryInfo(Path.Combine(target.FullName, dir.Name));
                CopyDirectory(dir, destinationSub);
            }
        }
    }
}