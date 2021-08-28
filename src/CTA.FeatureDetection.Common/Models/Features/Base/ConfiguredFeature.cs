using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Extensions;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features.Conditions.Base;
using CTA.FeatureDetection.Common.Reporting;

namespace CTA.FeatureDetection.Common.Models.Features.Base
{
    public class ConfiguredFeature : Feature
    {
        public override FeatureCategory FeatureCategory { get; }
        public override string Description { get; }
        public override bool IsLinuxCompatible { get; }

        public Condition Condition { get; set; }
        public ConditionGroup ConditionGroup { get; set; }
        public IEnumerable<ConditionGroup> ConditionGroups { get; set; }

        public ConfiguredFeature(
            FeatureScope featureScope, 
            string name, 
            FeatureCategory featureCategory, 
            string description, 
            bool isLinuxCompatible, 
            Condition condition)
        {
            FeatureScope = featureScope;
            Name = name;
            FeatureCategory = featureCategory;
            Description = description;
            IsLinuxCompatible = isLinuxCompatible;
            Condition = condition;
        }

        public ConfiguredFeature(
            FeatureScope featureScope,
            string name,
            FeatureCategory featureCategory,
            string description,
            bool isLinuxCompatible, 
            ConditionGroup conditionGroup)
        {
            FeatureScope = featureScope;
            Name = name;
            FeatureCategory = featureCategory;
            Description = description;
            IsLinuxCompatible = isLinuxCompatible;
            ConditionGroup = conditionGroup;
        }

        public ConfiguredFeature(
            FeatureScope featureScope,
            string name,
            FeatureCategory featureCategory,
            string description,
            bool isLinuxCompatible, 
            IEnumerable<ConditionGroup> conditionGroups)
        {
            FeatureScope = featureScope;
            Name = name;
            FeatureCategory = featureCategory;
            Description = description;
            IsLinuxCompatible = isLinuxCompatible;
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
