using System;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CTA.Rules.Models.Actions.VisualBasic
{
    public class AttributeAction : GenericAction
    {
        public Func<SyntaxGenerator, AttributeSyntax, AttributeSyntax> AttributeActionFunc { get; set; }
        public Func<SyntaxGenerator, AttributeListSyntax, AttributeListSyntax> AttributeListActionFunc { get; set; }

        public override bool Equals(object obj)
        {
            var action = (AttributeAction)obj;
            return action?.Key == Key
                   && action?.Value == Value
                   &&
                   (
                       (action.AttributeActionFunc != null && AttributeActionFunc != null &&
                        action.AttributeActionFunc.Method.Name == AttributeActionFunc.Method.Name)
                       ||
                       (action.AttributeListActionFunc != null && AttributeListActionFunc != null &&
                        action.AttributeListActionFunc.Method.Name == AttributeListActionFunc.Method.Name)
                   );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value, AttributeActionFunc?.Method.Name, AttributeListActionFunc?.Method.Name);
        }
    }
}
