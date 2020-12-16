using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Features.Conditions;
using CTA.FeatureDetection.Tests.Utils;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Models.Features.Conditions
{
    public class XmlFileQueryConditionTests
    {
        private readonly string _testProjectDirectory = TestUtils.GetTestAssemblySourceDirectory(typeof(TestUtils));

        [Test]
        public void IsConditionMet_Returns_True_If_Attribute_Values_Are_Found()
        {
            var testXmlFileDirectory = Path.Combine(_testProjectDirectory, "Examples", "XmlFiles");
            var conditionMetadata = new ConditionMetadata
            {
                Type = ConditionType.XmlFileQuery,
                MatchType = true,
                Properties = new Dictionary<string, object>
                {
                    { "FileNamePatterns", new [] {"TestAppConfig1.xml"} },
                    { "SearchPath", testXmlFileDirectory },
                    { "SearchOption", SearchOption.AllDirectories },
                    { "IgnoreCase", RegexOptions.IgnoreCase },
                    { "XmlElementPath", "configuration/connectionStrings/add" },
                    { "XmlElementAttributes", new Dictionary<string, object> 
                        {
                            { "providerName", "System.Data.SqlClient" }
                        }
                    }
                }
            };
            var xmlFileQueryCondition = new XmlFileQueryCondition(conditionMetadata);

            Assert.True(xmlFileQueryCondition.IsConditionMet(null));
        }
    }
}
