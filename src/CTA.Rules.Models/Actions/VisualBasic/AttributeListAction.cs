using System;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models.Actions.VisualBasic
{
    public class AttributeListAction : GenericAction
    {
        public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> AttributeListActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (Models.AttributeAction)obj;
            return action?.Key == this.Key
                && action?.Value == this.Value
                && action.AttributeListActionFunc != null && this.AttributeListActionFunc != null
                && action.AttributeListActionFunc.Method.Name == this.AttributeListActionFunc.Method.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, AttributeListActionFunc?.Method.Name);
        }
    }
}
