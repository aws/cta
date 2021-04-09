using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.Rules.Models
{
    public class IDEFileActions
    {
        public string FilePath { get; set; }
        public string Description { get; set; }
        public IList<TextChange> TextChanges { get; set; }
    }
}
