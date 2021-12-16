using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms.FileInformationModel
{
    public class FileInformation
    {
        private readonly string _relativePath;
        private readonly byte[] _fileBytes;

        public string RelativePath { get { return _relativePath; } }

        public byte[] FileBytes { get { return _fileBytes; } }

        public FileInformation(string relativePath, byte[] fileBytes)
        {
            _relativePath = relativePath;
            _fileBytes = fileBytes;
        }
    }
}
