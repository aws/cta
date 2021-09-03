using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.Common.Models.Features;
using CTA.FeatureDetection.Common.Reporting;
using CTA.FeatureDetection.Tests.TestBase;
using NUnit.Framework;

namespace CTA.FeatureDetection.Tests.FeatureDetection.Common.Reporting
{
    public class FeatureReportGeneratorTests : DetectAllFeaturesTestBase
    {
        [Test]
        public void GenerateCsvReport_Contains_All_PresentFeatures_From_Results()
        {
            var featureDetectionResults = new Dictionary<string, FeatureDetectionResult>
            {
                {"AuthProject.csproj", _formsAuthenticationFeatureDetectionResult},
                {"MyMvcProject.csproj", _mvcFeatureDetectionResult}
            };
            var generatedReport = FeatureReportGenerator.GenerateCsvReport(featureDetectionResults, FeatureDetector.LoadedFeatureSet);
            var expectedReport = @"FeatureCategory,ProjectName,FeatureName,Description,IsLinuxCompatible
ProjectType,AuthProject.csproj,AspNetMvcFeature,This project type is ASP.NET MVC.,True
AuthType,AuthProject.csproj,FormsAuthenticationFeature,This project uses the Forms Authentication method.,True
AuthType,AuthProject.csproj,FormsAuthenticationWithMembershipFeature,This project uses the Forms Authentication method with membership.,False
ProjectType,MyMvcProject.csproj,AspNetMvcFeature,This project type is ASP.NET MVC.,True
";

            Assert.AreEqual(expectedReport, generatedReport);
        }

        [Test]
        public void GenerateCsvReport_Returns_Empty_String_If_LoadedFeatures_Is_Null()
        {
            var featureDetectionResults = new Dictionary<string, FeatureDetectionResult>
            {
                {"AuthProject.csproj", _formsAuthenticationFeatureDetectionResult},
                {"MyMvcProject.csproj", _mvcFeatureDetectionResult}
            };
            FeatureSet nullFeatureSet = null;
            var generatedReport = FeatureReportGenerator.GenerateCsvReport(featureDetectionResults, nullFeatureSet);
            var expectedReport = string.Empty;

            Assert.AreEqual(expectedReport, generatedReport);
        }

        [Test]
        public void GenerateCsvReport_Returns_Report_Header_If_LoadedFeatures_Is_Empty()
        {
            using var sw = new StringWriter();
            using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
            csv.WriteRecords(new List<FeatureReportRecord>());
            var expectedReport = sw.ToString();

            var featureDetectionResults = new Dictionary<string, FeatureDetectionResult>
            {
                {"AuthProject.csproj", _formsAuthenticationFeatureDetectionResult},
                {"MyMvcProject.csproj", _mvcFeatureDetectionResult}
            };
            var emptyFeatureSet = new FeatureSet();
            var generatedReport = FeatureReportGenerator.GenerateCsvReport(featureDetectionResults, emptyFeatureSet);

            Assert.AreEqual(expectedReport, generatedReport);
        }

        [Test]
        public void GenerateCsvReport_Returns_Empty_String_If_FeatureDetectionResults_Is_Null()
        {
            Dictionary<string, FeatureDetectionResult> featureDetectionResults = null;
            var generatedReport = FeatureReportGenerator.GenerateCsvReport(featureDetectionResults, FeatureDetector.LoadedFeatureSet);
            var expectedReport = string.Empty;

            Assert.AreEqual(expectedReport, generatedReport);
        }

        [Test]
        public void GenerateCsvReport_Returns_Report_Header_If_FeatureDetectionResults_Is_Empty()
        {
            using var sw = new StringWriter();
            using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
            csv.WriteRecords(new List<FeatureReportRecord>());
            var expectedReport = sw.ToString();

            var featureDetectionResults = new Dictionary<string, FeatureDetectionResult>();
            var generatedReport = FeatureReportGenerator.GenerateCsvReport(featureDetectionResults, FeatureDetector.LoadedFeatureSet);

            Assert.AreEqual(expectedReport, generatedReport);
        }

        [Test]
        public void ExportReport_Writes_Report_To_Specified_Path()
        {
            var destinationPath = "ExportedReport.csv";
            var report = @"FeatureCategory,ProjectName,FeatureName,Description,IsLinuxCompatible
ProjectType,MyMvcProject.csproj,AspNetMvcFeature,This project type is ASP.NET MVC.,True
ProjectType,AuthProject.csproj,AspNetMvcFeature,This project type is ASP.NET MVC.,True
AuthType,AuthProject.csproj,FormsAuthenticationFeature,This project uses the Forms Authentication method.,True
AuthType,AuthProject.csproj,FormsAuthenticationWithMembershipFeature,This project uses the Forms Authentication method with membership.,False
";
            try
            {
                // Write report to file
                FeatureReportGenerator.ExportReport(report, null, destinationPath);

                // Read back the contents of file we just wrote
                var actualReport = File.ReadAllText(destinationPath);

                // Assert contents are identical to original
                Assert.AreEqual(report, actualReport);
            }
            catch
            {
                // Clean up file on exception
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
            }
        }
    }
}
