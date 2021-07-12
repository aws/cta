
using System;
using Codelyzer.Analysis.Model;

namespace CTA.Rules.Models
{
    public class PackageAction
    {
        public string Name { get; set; }
        public string OriginalVersion { get; set; }
        public string Version = "*";
        public TextSpan TextSpan { get; set; }

        public PackageAction Clone() => (PackageAction)this.MemberwiseClone();
        public override bool Equals(object obj)
        {
            var action = (PackageAction)obj;
            return action?.Name == this.Name
                && action?.Version == this.Version;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name?.GetHashCode() ?? 0,
                3 * (!string.IsNullOrEmpty(Version) ? Version.GetHashCode() : 0));
        }
    }
}
