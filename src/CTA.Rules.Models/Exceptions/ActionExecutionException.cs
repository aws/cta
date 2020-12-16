using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.Rules.Models
{
    public class ActionExecutionException : Exception
    {
        public ActionExecutionException(string actionType, string actionName, Exception exception) 
            : base(GetActionExecutionException(actionType, actionName), exception)
        {

        }
        public ActionExecutionException(string message) : base(message)
        {
        }

        public static string GetActionExecutionException(string actionType, string actionName)
        {
            return string.Format("Error while executing action {0}-{1}", actionType, actionName);
        }
    }
}
