using CTA.Rules.Models.VisualBasic;

namespace CTA.Rules.Models.RulesFiles
{
    public class RulesFileLoaderResponse
    {
        public RootNodes CsharpRootNodes { get; set; }
        public VisualBasicRootNodes VisualBasicRootNodes { get; set; }
    }
}
