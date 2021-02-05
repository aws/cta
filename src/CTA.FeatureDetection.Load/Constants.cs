using System.IO;
using System.Reflection;

namespace CTA.FeatureDetection.Load
{
    internal class Constants
    {
        public const string TemplatesFilePath = "Templates";
        public static readonly string DefaultFeatureConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), TemplatesFilePath, "default.json");
    }
}