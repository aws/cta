using System;
using System.IO;
using System.Linq;
using CTA.WebForms.Extensions;

namespace CTA.WebForms.Helpers
{
    public static class FilePathHelper
    {
        private const string PreviousDirPathSymbol = "..";
        private const string CurrentDirPathSymbol = ".";
        private const string FileNameDoesNotContainExtensionError = 
            "Cannot alter file name, file name {0} does not end with extension {1}";
        
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

        public static string GetNamespaceFromRelativeFilePath(string filePath, string projectName)
        {
            var cleanedFilePath = filePath.RemoveOuterQuotes();
            var directoryName = Path.GetDirectoryName(cleanedFilePath);

            return string.IsNullOrEmpty(directoryName)
                ? directoryName
                : directoryName.Replace("~", projectName).Replace(Path.DirectorySeparatorChar.ToString(), ".");
        }

        public static string RemoveDuplicateDirectories(string filePath)
        {
            var directories = filePath.Split(Path.DirectorySeparatorChar);
            var lastDirectory = string.Empty;
            var result = string.Empty;

            foreach (var directory in directories)
            {
                if (!directory.Equals(lastDirectory))
                {
                    result = string.IsNullOrEmpty(result) ? directory : Path.Combine(result, directory);
                }

                lastDirectory = directory;
            }

            return result;
        }

        public static bool IsSubDirectory(string basePath, string otherPath)
        {
            var relativePath = Path.GetRelativePath(basePath, otherPath);
            var pathSeparator = Path.PathSeparator;
            var firstDir = relativePath.Split(Path.DirectorySeparatorChar).FirstOrDefault();

            if (relativePath.Equals(otherPath)
                || relativePath.Equals(CurrentDirPathSymbol)
                || (firstDir?.Equals(PreviousDirPathSymbol) ?? true))
            {
                return false;
            }

            return true;
        }
    }
}
