using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.FeatureDetection.Common.Models.Features.Conditions.Base;

[assembly: InternalsVisibleTo("CTA.FeatureDetection.Tests")]
namespace CTA.FeatureDetection.Load.Factories
{
    internal class ConfiguredFeatureFactory
    {
        /// <summary>
        /// Factory method that dynamically instantiates a configured feature using condition metadata
        /// </summary>
        /// <param name="featureScope">The scope in which to look for this feature</param>
        /// <param name="name">Name of the feature</param>
        /// <param name="conditionMetadata">Metadata defining the feature condition</param>
        /// <returns>Instance of the defined feature</returns>
        public static ConfiguredFeature GetInstance(FeatureScope featureScope, string name, ConditionMetadata conditionMetadata)
        {
            var condition = ConditionFactory.GetCondition(conditionMetadata);
            return new ConfiguredFeature(featureScope, name, condition);
        }

        /// <summary>
        /// Factory method that dynamically instantiates a configured feature using condition group metadata
        /// </summary>
        /// <param name="featureScope">The scope in which to look for this feature</param>
        /// <param name="name">Name of the feature</param>
        /// <param name="conditionGroupMetadata">Metadata defining the feature condition group</param>
        /// <returns>Instance of the defined feature</returns>
        public static ConfiguredFeature GetInstance(FeatureScope featureScope, string name, ConditionGroupMetadata conditionGroupMetadata)
        {
            var conditionGroup = new ConditionGroup
            {
                JoinOperator = conditionGroupMetadata.JoinOperator,
                Conditions = ConditionFactory.GetConditions(conditionGroupMetadata.Conditions)
            };
            return new ConfiguredFeature(featureScope, name, conditionGroup);
        }

        /// <summary>
        /// Factory method that dynamically instantiates a configured feature using a collection of condition group metadata
        /// </summary>
        /// <param name="featureScope">The scope in which to look for this feature</param>
        /// <param name="name">Name of the feature</param>
        /// <param name="conditionGroupsMetadata">Metadata defining the feature condition groups</param>
        /// <returns>Instance of the defined feature</returns>
        public static ConfiguredFeature GetInstance(FeatureScope featureScope, string name, IEnumerable<ConditionGroupMetadata> conditionGroupsMetadata)
        {
            var conditionGroups = conditionGroupsMetadata.Select(conditionGroupMetadata =>
                new ConditionGroup
                {
                    JoinOperator = conditionGroupMetadata.JoinOperator,
                    Conditions = ConditionFactory.GetConditions(conditionGroupMetadata.Conditions)
                }
            );
            return new ConfiguredFeature(featureScope, name, conditionGroups);
        }
    }
}