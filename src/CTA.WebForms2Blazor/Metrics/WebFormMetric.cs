using CTA.Rules.Models;

namespace CTA.WebForms2Blazor.Metrics
{
    public class WebFormMetric
    {
        public WebFormsActionType ActionName { get; set; }
        public string ChildActionName { get; set; }
        public string NodeName { get; set; }

        public WebFormMetric(WebFormsActionType actionName, string childActionName, string nodeName = "")
        {
            ActionName = actionName;
            ChildActionName = childActionName;
            NodeName = nodeName;
        }
    }
}
