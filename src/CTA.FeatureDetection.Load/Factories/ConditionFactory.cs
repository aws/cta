using System;
using System.Linq;
using System.Reflection;
using CTA.FeatureDetection.Common.Exceptions;
using CTA.FeatureDetection.Common.Models.Enums;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Features.Conditions.Base;

[assembly: InternalsVisibleTo("CTA.FeatureDetection.Tests")]
namespace CTA.FeatureDetection.Load.Factories
{
    internal class ConditionFactory
    {
        /// <summary>
        /// Get instance of a condition from condition metadata
        /// </summary>
        /// <param name="conditionMetadata">Metadata specifying the type of condition to instantiate and its property values</param>
        /// <returns>Condition instance</returns>
        public static Condition GetCondition(ConditionMetadata conditionMetadata)
        {
            var assembly = Assembly.GetAssembly(typeof(Condition));
            var className = ConditionTypeEnumToClassName(conditionMetadata.Type);
            var conditionType = assembly.GetTypes().SingleOrDefault(t => t.Name == className);
            if (conditionType == null)
            {
                throw new ClassNotFoundException(assembly, className);
            }

            var conditionInstance = Activator.CreateInstance(conditionType, conditionMetadata) as Condition;
            if (conditionInstance == null)
            {
                throw new InvalidCastException($"Type {className} cannot be cast to {typeof(Condition)}");
            }

            return conditionInstance;
        }

        /// <summary>
        /// Get a collection of condition instances from condition metadata
        /// </summary>
        /// <param name="conditionsMetadata">Collection of metadata specifying the types of conditions to instantiate and their properties' values</param>
        /// <returns>Collection of condition instances</returns>
        public static IEnumerable<Condition> GetConditions(IEnumerable<ConditionMetadata> conditionsMetadata)
        {
            return conditionsMetadata.Select(GetCondition);
        }

        private static string ConditionTypeEnumToClassName(ConditionType conditionType)
        {
            return $"{conditionType}Condition";
        }
    }
}