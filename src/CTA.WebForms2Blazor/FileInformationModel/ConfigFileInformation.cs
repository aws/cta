using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms2Blazor.FileInformationModel
{
    class ConfigFileInformation : FileInformation
    {
        public ConfigFileInformation(string relativePath, byte[] fileBytes) : base(relativePath, fileBytes)
        {

        }
    }
}
