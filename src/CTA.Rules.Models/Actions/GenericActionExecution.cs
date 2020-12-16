namespace CTA.Rules.Models
{
    public class GenericActionExecution : GenericAction
    {
        public GenericActionExecution()
        {

        }

        public GenericActionExecution(GenericAction genericAction) : this(genericAction, string.Empty)
        {
        }

        public GenericActionExecution(GenericAction genericAction, string filePath)
        {
            this.Name = genericAction.Name;
            this.Type = genericAction.Type;
            this.Key = genericAction.Key;
            this.Value = genericAction.Value;
            this.Description = genericAction.Description;
            this.TextSpan = genericAction.TextSpan;
            this.ActionValidation = genericAction.ActionValidation;
            this.FilePath = filePath;
        }
        
        public string FilePath { get; set; }
        public int TimesRun { get; set; }
        public int InvalidExecutions { get; set; }
    }
}
