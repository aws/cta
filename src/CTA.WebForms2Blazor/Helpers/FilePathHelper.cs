using System;
using System.IO;
using CTA.FeatureDetection.Common.Extensions;
using CTA.WebForms2Blazor.Extensions;

namespace CTA.WebForms2Blazor.Helpers
{
    public static class FilePathHelper
    {
        private const string FileNameDoesNotContainExtensionError = 
            "Cannot alter file name, file name {0} does not end with extension {1}";
        private const string FileNameDoesNotContainDirectoryError =
            "Cannot convert file name to namespace, file path {0} does not have a directory";
        
        public static string AlterFileName(string oldFilePath, string newFileName = null, string oldExtension = null, string newExtension = null)
        {
            var actualOldExtension = oldExtension ?? Path.GetExtension(oldFilePath);
            var oldFileDirectory = Path.GetDirectoryName(oldFilePath);
            var oldFileName = Path.GetFileName(oldFilePath);

            if (!oldFileName.EndsWith(actualOldExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException(string.Format(FileNameDoesNotContainExtensionError, oldFileName, actualOldExtension));
            }

            var oldFileNameNoExtension = oldFileName.Substring(0, oldFileName.Length - actualOldExtension.Length);
            var newFileNameWithExtension = (newFileName ?? oldFileNameNoExtension) + (newExtension ?? actualOldExtension);

            if (!string.IsNullOrEmpty(oldFileDirectory))
            {
                return Path.Combine(oldFileDirectory, newFileNameWithExtension);
            }

            return newFileNameWithExtension;
        }
        
        public static string FilePathToNamespace(string filePath, string projectName)
        {
            string res = filePath.RemoveOuterQuotes();
            res = Path.GetDirectoryName(res);
            
            return res.IsNullOrEmpty() ? 
                string.Format(FileNameDoesNotContainDirectoryError, filePath) : 
                res.Replace("~", projectName).Replace("/", ".");
        }
    }
}
