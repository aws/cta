using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CTA.WebForms2Blazor.Helpers
{
    public static class FilePathHelper
    {
        private const string FileNameDoesNotContainExtensionError = "Cannot alter file name, file name {0} does not contain extension {1}";

        public static string AlterFileName(string oldFilePath, string newFileName = null, string oldExtension = null, string newExtension = null)
        {
            var actualOldExtension = oldExtension ?? Path.GetExtension(oldFilePath);
            var oldFileDirectory = Path.GetDirectoryName(oldFilePath);
            var oldFileName = Path.GetFileName(oldFilePath);

            if (!oldFileName.Contains(actualOldExtension))
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
    }
}
