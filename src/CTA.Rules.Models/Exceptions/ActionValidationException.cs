using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.Rules.Models
{
    public class ActionValidationException : Exception
    {
        public ActionValidationException(string actionType, string actionName) 
            : base(GetActionValidationException(actionType, actionName))
        {

        }

        public static string GetActionValidationException(string actionType, string actionName)
        {
            return string.Format("Error while validating action {0}-{1}", actionType, actionName);
        }
    }
}
