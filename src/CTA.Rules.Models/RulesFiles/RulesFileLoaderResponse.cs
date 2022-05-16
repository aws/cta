using CTA.Rules.Models.VisualBasic;

namespace CTA.Rules.Models.RulesFiles;

public class RulesFileLoaderResponse
{
    public CsharpRootNodes CsharpRootNodes { get; set; }
    public VisualBasicRootNodes VisualBasicRootNodes { get; set; }
}
