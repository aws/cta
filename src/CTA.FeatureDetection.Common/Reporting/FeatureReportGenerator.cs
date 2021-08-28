using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.Common.Models.Features;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.Rules.Config;

namespace CTA.FeatureDetection.Common.Reporting
{
    public class FeatureReportGenerator
    {
        private const string DefaultFeatureReportPath = "DetectedFeaturesReport.csv";

        public static string GenerateCsvReport(
            IDictionary<string, FeatureDetectionResult> featureDetectionResults, 
            FeatureSet loadedFeatures)
        {
            var featureLookup = ConvertLoadedFeaturesToDict(loadedFeatures);
            var featureReportRecords = featureDetectionResults.SelectMany(kvp =>
            {
                var (projectName, featureDetectionResult) = kvp;
                return ConvertFeatureResultsToRecords(projectName, featureDetectionResult, featureLookup);
            });

            using var sw = new StringWriter();
            using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
            csv.WriteRecords(featureReportRecords);

            return sw.ToString();
        }

        /// <summary>
        /// Saves string content to a file.
        /// </summary>
        /// <param name="csvReport"></param>
        /// <param name="exportPath"></param>
        public static void ExportReport(string csvReport, string exportPath = null)
        {
            exportPath = string.IsNullOrEmpty(exportPath) ? DefaultFeatureReportPath : exportPath;
            Utils.ThreadSafeExportStringToFile(exportPath, csvReport);
        }

        /// <summary>
        /// Converts loaded features into a dictionary object.
        /// This can be used to lookup a feature's metadata based on the feature name.
        /// </summary>
        /// <param name="loadedFeatures">Features loaded into the FeatureDetector</param>
        /// <returns>A dictionary mapping a feature's metadata to its name</returns>
        private static IDictionary<string, Feature> ConvertLoadedFeaturesToDict(FeatureSet loadedFeatures)
        {
            return loadedFeatures.AllFeatures
                .ToDictionary(feature => feature.Name, feature => feature);
        }

        // TODO: Properly assign FeatureCategory and Description
        private static IEnumerable<FeatureReportRecord> ConvertFeatureResultsToRecords(
            string projectName,
            FeatureDetectionResult featureDetectionResult,
            IDictionary<string, Feature> featureLookup)
        {
            return featureDetectionResult.PresentFeatures.Select(feature =>
            {
                var featureMetadata = featureLookup[feature];
                return new FeatureReportRecord
                {
                    ProjectName = projectName,
                    FeatureCategory = featureMetadata.FeatureCategory,
                    FeatureName = featureMetadata.Name,
                    Description = featureMetadata.Description,
                    IsLinuxCompatible = featureMetadata.IsLinuxCompatible
                };
            });
        }
    }
}
