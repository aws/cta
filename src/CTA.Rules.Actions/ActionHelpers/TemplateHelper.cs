using System.IO;
using System.Reflection;
using CTA.Rules.Models;
using CTA.Rules.Config;

namespace CTA.Rules.Config
{
    public class TemplateHelper
    {
        /// <summary>
        /// Gets the content of a template file 
        /// </summary>
        /// <param name="templateUrl">The file directory</param>
        /// <param name="projectNamespace">The project namespace to be used when creating the template file</param>
        /// <returns></returns>
        internal static string GetTemplateFileContent(string projectNamespace, ProjectType projectType, string fileName)
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), GetProjectTypeTemplateDir(projectType), fileName);
            string fileContent;

            fileContent = File.ReadAllText(file);

            return fileContent.Replace("/*", "").Replace("*/", "").Replace(Constants.NameSpacePlaceHolder, projectNamespace);
        }

        /// <summary>
        /// Gets the directory containing the project template files, based on the project type
        /// </summary>
        /// <param name="projectType">The project type</param>
        /// <returns></returns>
        private static string GetProjectTypeTemplateDir(ProjectType projectType)
        {
            return Path.Combine(Constants.Templates, projectType.ToString().ToLower());
        }
    }
}
