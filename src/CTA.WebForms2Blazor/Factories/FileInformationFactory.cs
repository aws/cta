using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using CTA.WebForms2Blazor.FileInformationModel;

namespace CTA.WebForms2Blazor.Factories
{
    public static class FileInformationFactory
    {
        public static FileInformation Build(FileInfo document)
        {
            // NOTE
            // Existing Type:   FileInfo = System.IO.FileInfo
            // Our New Type:    FileInformation = CTA.WebForms2Blazor.FileInformationModel.FileInformation

            // Add logic to determine the type of FileInformation
            // object to create, likely using the file type specified
            // in the FileInfo object

            throw new NotImplementedException();
        }

        public static IEnumerable<FileInformation> BuildMany(IEnumerable<FileInfo> documents)
        {
            return documents.Select(document => Build(document));
        }
    }
}
