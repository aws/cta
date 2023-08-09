using System;
using System.Collections.Generic;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Configuration;
using Newtonsoft.Json.Linq;

namespace CTA.FeatureDetection.Common.Models.Features.Conditions.Base
{
    public abstract class Condition
    {
        public bool MatchType { get; set; }

        protected Condition(ConditionMetadata conditionMetadata)
        {
            MatchType = conditionMetadata.MatchType;
            PopulateProperties(conditionMetadata.Properties);
        }

        public abstract bool IsConditionMet(Codelyzer.Analysis.Model.AnalyzerResult analyzerResult);

        private void PopulateProperties(Dictionary<string, object> properties)
        {
            var thisType = GetType();
            foreach (var property in properties)
            {
                var propertyName = property.Key;
                var propertyInfo = thisType.GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    object propertyValue;
                    if (property.Value is JObject dictionaryValue)
                    {
                        propertyValue = dictionaryValue.ToObject<Dictionary<string, object>>();
                    }
                    else if (property.Value is JArray arrayValue)
                    {
                        // convert JArray to destination type
                        propertyValue = propertyInfo.GetValue(this) switch
                        {
                            string[] stringArray => arrayValue.ToObject<string[]>(),
                            bool[] boolArray => arrayValue.ToObject<bool[]>(),
                            int[] intArray => arrayValue.ToObject<int[]>(),
                            long[] longArray => arrayValue.ToObject<long[]>(),
                            decimal[] decimalArray => arrayValue.ToObject<decimal[]>(),
                            _ => arrayValue.ToObject<object[]>()
                        };
                    }
                    else
                    {
                        propertyValue = property.Value;
                    }

                    try
                    {
                        propertyInfo.SetValue(this, propertyValue);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"Cannot assign {propertyValue} ({propertyValue.GetType()} type) " +
                                                    $"to property {propertyName} ({propertyInfo.PropertyType} type).", ex);
                    }
                }
                else
                {
                    throw new MissingMemberException($"Property {propertyName} does not exist in {thisType}.");
                }
            }
        }
    }
}
