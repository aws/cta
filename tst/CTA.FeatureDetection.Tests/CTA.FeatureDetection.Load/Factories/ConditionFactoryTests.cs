using CTA.FeatureDetection.Common.Models.Features.Conditions;
using CTA.FeatureDetection.Load.Factories;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;
using System.Linq;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Load.Factories
{
    public class ConditionFactoryTests : ConditionTestBase
    {

        [Test]
        public void GetCondition_Returns_Condition_From_Condition_Metadata()
        {
            var condition = ConditionFactory.GetCondition(ConditionMetadata);

            Assert.NotNull(condition);
            Assert.True(condition is XmlFileQueryCondition);
        }

        [Test]
        public void GetConditions_Returns_Conditions_From_Conditions_Metadata()
        {
            var conditions = ConditionFactory.GetConditions(ConditionGroupMetadata.Conditions).ToList();

            Assert.True(conditions.Count == 2);
            Assert.True(conditions.All(c => c.GetType() == typeof(XmlFileQueryCondition)));
        }
    }
}