using System;
using System.IO;
using System.Collections.Generic;

namespace CTA.WebForms2Blazor
{
    public class WebFormsProjectAnalyzer
    {
        private readonly string _inputProjectPath;

        public string InputProjectPath { get { return _inputProjectPath; } }

        public WebFormsProjectAnalyzer(string inputProjectPath)
        {
            _inputProjectPath = inputProjectPath;
        }

        public IEnumerable<FileInfo> GetProjectFileInfo()
        {
            throw new NotImplementedException();
        }
    }
}
