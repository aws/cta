using System;

namespace CTA.WebForms2Blazor.Helpers.ControlHelpers
{
    public class ViewLayerControlAttribute
    {
        public string Name { get; }
        public string Value { get; }

        public ViewLayerControlAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name + "=" + Value;
        }

        private bool Equals(ViewLayerControlAttribute other)
        {
            if (other is null)
            {
                return false;
            }
            return string.Equals(this.Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
        }
        public override bool Equals(object obj) => Equals(obj as ViewLayerControlAttribute);
        public override int GetHashCode() => HashCode.Combine(Name);
    }
}
