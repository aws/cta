using System;
using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Enums;

namespace CTA.FeatureDetection.Common.Models.Features.Conditions.Base
{
    public class ConditionGroup
    {
        public JoinOperator JoinOperator { get; set; }
        public IEnumerable<Condition> Conditions { get; set; }

        public bool AreConditionsMet(AnalyzerResult analyzerResult)
        {
            return JoinOperator switch
            {
                JoinOperator.All => EvaluateWithAll(analyzerResult),
                JoinOperator.Any => EvaluateWithAny(analyzerResult),
                JoinOperator.None => EvaluateWithNone(analyzerResult),
                _ => throw new ArgumentException($"Unsupported {JoinOperator.GetType()} type: {JoinOperator}"),
            };
        }

        private bool EvaluateWithAll(AnalyzerResult analyzerResult)
        {
            return Conditions.All(condition => condition.IsConditionMet(analyzerResult));
        }

        private bool EvaluateWithAny(AnalyzerResult analyzerResult)
        {
            return Conditions.Any(condition => condition.IsConditionMet(analyzerResult));
        }

        private bool EvaluateWithNone(AnalyzerResult analyzerResult)
        {
            return !Conditions.Any(condition => condition.IsConditionMet(analyzerResult));
        }
    }
}