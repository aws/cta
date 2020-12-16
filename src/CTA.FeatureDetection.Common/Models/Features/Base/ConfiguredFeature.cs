using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features.Conditions.Base;

namespace CTA.FeatureDetection.Common.Models.Features.Base
{
    public class ConfiguredFeature : Feature
    {
        public Condition Condition { get; set; }
        public ConditionGroup ConditionGroup { get; set; }
        public IEnumerable<ConditionGroup> ConditionGroups { get; set; }

        public ConfiguredFeature(FeatureScope featureScope, string name, Condition condition)
        {
            FeatureScope = featureScope;
            Name = name;
            Condition = condition;
        }

        public ConfiguredFeature(FeatureScope featureScope, string name, ConditionGroup conditionGroup)
        {
            FeatureScope = featureScope;
            Name = name;
            ConditionGroup = conditionGroup;
        }

        public ConfiguredFeature(FeatureScope featureScope, string name, IEnumerable<ConditionGroup> conditionGroups)
        {
            FeatureScope = featureScope;
            Name = name;
            ConditionGroups = conditionGroups;
        }

        /// <summary>
        /// Determines if the Configured Feature is present in a project
        /// </summary>
        /// <param name="analyzerResult">Project analysis results from Codelyzer</param>
        /// <returns>Whether or not the feature exists in the project</returns>
        public override bool IsPresent(AnalyzerResult analyzerResult)
        {
            if (!ConditionGroups.IsNullOrEmpty())
            {
                return ConditionGroups.All(cg => cg.AreConditionsMet(analyzerResult));
            }

            if (ConditionGroup != null)
            {
                return ConditionGroup.AreConditionsMet(analyzerResult);
            }

            if (Condition != null)
            {
                return Condition.IsConditionMet(analyzerResult);
            }

            return false;
        }
    }
}