using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    class ProjectFileInformation : FileInformation
    {
        public ProjectFileInformation(string relativePath, byte[] fileBytes) : base(relativePath, fileBytes)
        {

        }
    }
}
