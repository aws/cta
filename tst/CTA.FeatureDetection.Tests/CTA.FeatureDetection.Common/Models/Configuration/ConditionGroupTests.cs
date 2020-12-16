using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features.Conditions.Base;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Models.Configuration
{
    public class ConditionGroupTests : ConditionTestBase
    {
        [Test]
        public void ConditionGroup_With_Any_JoinOperator_Evaluates_Conditions_Like_OR()
        {
            var conditionGroup_True_True = new ConditionGroup
            {
                JoinOperator = JoinOperator.Any,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysTrue,
                    ConditionThatIsAlwaysTrue,
                }
            };
            var conditionGroup_True_False = new ConditionGroup
            {
                JoinOperator = JoinOperator.Any,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysTrue,
                    ConditionThatIsAlwaysFalse,
                }
            };
            var conditionGroup_False_False = new ConditionGroup
            {
                JoinOperator = JoinOperator.Any,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysFalse,
                    ConditionThatIsAlwaysFalse,
                }
            };

            Assert.True(conditionGroup_True_True.AreConditionsMet(AnalyzerResult));
            Assert.True(conditionGroup_True_False.AreConditionsMet(AnalyzerResult));
            Assert.False(conditionGroup_False_False.AreConditionsMet(AnalyzerResult));
        }

        [Test]
        public void ConditionGroup_With_All_JoinOperator_Evaluates_Conditions_Like_AND()
        {
            var conditionGroup_True_True = new ConditionGroup
            {
                JoinOperator = JoinOperator.All,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysTrue,
                    ConditionThatIsAlwaysTrue
                }
            };
            var conditionGroup_True_False = new ConditionGroup
            {
                JoinOperator = JoinOperator.All,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysTrue,
                    ConditionThatIsAlwaysFalse
                }
            };
            var conditionGroup_False_False = new ConditionGroup
            {
                JoinOperator = JoinOperator.All,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysFalse,
                    ConditionThatIsAlwaysFalse
                }
            };

            Assert.True(conditionGroup_True_True.AreConditionsMet(AnalyzerResult));
            Assert.False(conditionGroup_True_False.AreConditionsMet(AnalyzerResult));
            Assert.False(conditionGroup_False_False.AreConditionsMet(AnalyzerResult));
        }

        [Test]
        public void ConditionGroup_With_None_JoinOperator_Evaluates_Conditions_Like_NAN()
        {
            var conditionGroup_True_True = new ConditionGroup
            {
                JoinOperator = JoinOperator.None,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysTrue,
                    ConditionThatIsAlwaysTrue
                }
            };
            var conditionGroup_True_False = new ConditionGroup
            {
                JoinOperator = JoinOperator.None,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysTrue,
                    ConditionThatIsAlwaysFalse
                }
            };
            var conditionGroup_False_False = new ConditionGroup
            {
                JoinOperator = JoinOperator.None,
                Conditions = new[]
                {
                    ConditionThatIsAlwaysFalse,
                    ConditionThatIsAlwaysFalse
                }
            };

            Assert.False(conditionGroup_True_True.AreConditionsMet(AnalyzerResult));
            Assert.False(conditionGroup_True_False.AreConditionsMet(AnalyzerResult));
            Assert.True(conditionGroup_False_False.AreConditionsMet(AnalyzerResult));
        }
    }
}