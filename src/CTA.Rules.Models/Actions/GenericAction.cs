using Codelyzer.Analysis.Model;


namespace CTA.Rules.Models
{
    public class GenericAction
    {
        public GenericAction()
        {
            ActionValidation = new ActionValidation();
        }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public TextSpan TextSpan { get; set; }
        public ActionValidation ActionValidation { get; set; }

        public T Clone<T>() => (T)this.MemberwiseClone();

        public virtual GenericAction Copy()
        {
            GenericAction copy = new GenericAction();
            copy.Name = this.Name;
            copy.Type = this.Type;
            copy.Key = this.Key;
            copy.Value = this.Value;
            copy.Description = this.Description;
            copy.TextSpan = this.TextSpan;
            copy.ActionValidation = this.ActionValidation;
            return copy;
        }
    }
}
