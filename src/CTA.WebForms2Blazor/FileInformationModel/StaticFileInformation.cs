using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    public class StaticFileInformation : FileInformation
    {
        public StaticFileInformation(string relativePath, byte[] fileBytes) : base(relativePath, fileBytes)
        {

        }
    }
}
