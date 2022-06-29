using System;

namespace CTA.Rules.Common.Helpers
{
    public class VisualBasicUtils
    {
        /// <summary>
        /// Checks project file for visual basic project extension
        /// </summary>
        /// <param name="projectFilePath">projectFilePath</param>
        /// <returns>True if the file path contains the visual basic project extension</returns>
        public static bool IsVisualBasicProject(string projectFilePath)
        {
            return projectFilePath.EndsWith(".vbproj", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
