
using Codelyzer.Analysis.Model;
using System.Security;

namespace CTA.Rules.Models
{
    public class PackageAction
    {
        public string Name { get; set; }
        public string PreviousVersion { get; set; }
        public string Version = "*"; 
        public TextSpan TextSpan { get; set; }
        public override bool Equals(object obj)
        {
            var action = (PackageAction)obj;
            return action.Name == this.Name
                && action.Version == this.Version;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode()
                + 3 * (!string.IsNullOrEmpty(Version) ? Version.GetHashCode() : 0);
        }
    }
}