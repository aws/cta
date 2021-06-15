using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    class CodeFileInformation : FileInformation
    {
        public CodeFileInformation(string relativePath, byte[] fileBytes) : base(relativePath, fileBytes)
        {

        }
    }
}
