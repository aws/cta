using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    class ViewFileInformation : FileInformation
    {
        public ViewFileInformation(string relativePath, byte[] fileBytes) : base(relativePath, fileBytes)
        {

        }
    }
}
