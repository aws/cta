using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features.Conditions.Base;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace CTA.FeatureDetection.Tests.TestBase
{
    public class ConditionTestBase
    {
        private static Mock<Condition> _conditionThatIsAlwaysTrue;
        private static Mock<Condition> _conditionThatIsAlwaysFalse;

        protected AnalyzerResult AnalyzerResult { get; private set; }
        protected Condition ConditionThatIsAlwaysTrue => _conditionThatIsAlwaysTrue.Object;
        protected Condition ConditionThatIsAlwaysFalse => _conditionThatIsAlwaysFalse.Object;
        protected ConditionMetadata ConditionMetadata { get; set; }
        protected ConditionGroupMetadata ConditionGroupMetadata { get; set; }
        protected IEnumerable<ConditionGroupMetadata> ConditionGroupsMetadata { get; set; }

        [SetUp]
        public void Setup()
        {
            AnalyzerResult = new AnalyzerResult();
            var conditionMetadata = new ConditionMetadata();
            _conditionThatIsAlwaysTrue = new Mock<Condition>(conditionMetadata);
            _conditionThatIsAlwaysTrue.Setup(x => x.IsConditionMet(AnalyzerResult)).Returns(true);

            _conditionThatIsAlwaysFalse = new Mock<Condition>(conditionMetadata);
            _conditionThatIsAlwaysFalse.Setup(x => x.IsConditionMet(AnalyzerResult)).Returns(false);

            ConditionMetadata = new ConditionMetadata
            {
                Type = ConditionType.XmlFileQuery,
                Properties = new Dictionary<string, object>
                {
                    {"FileNamePatterns", new[] {"SomeFile", "SomeOtherFile"}},
                    {"XmlElementPath", "path/to/element"}
                }
            };
            ConditionGroupMetadata = new ConditionGroupMetadata
            {
                JoinOperator = JoinOperator.All,
                Conditions = new[]
                {
                    ConditionMetadata,
                    ConditionMetadata
                }
            };
            ConditionGroupsMetadata = new[]
            {
                ConditionGroupMetadata,
                ConditionGroupMetadata
            };
        }
    }
}
