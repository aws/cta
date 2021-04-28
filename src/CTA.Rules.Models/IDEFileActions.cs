using System;
using System.Collections.Generic;
using System.Text;
using Codelyzer.Analysis.Model;

namespace CTA.Rules.Models
{
    public class IDEFileActions
    {
        public string FilePath { get; set; }
        public TextSpan TextSpan { get; set; }
        public string Description { get; set; }
        public IList<TextChange> TextChanges { get; set; }
    }
}
