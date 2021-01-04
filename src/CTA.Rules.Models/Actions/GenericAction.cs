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

        public GenericAction Clone() => (GenericAction)this.MemberwiseClone();
    }
}
