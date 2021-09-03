using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CTA.FeatureDetection.Common.Models;
using CTA.FeatureDetection.Common.Models.Features;
using CTA.FeatureDetection.Common.Models.Features.Base;
using CTA.Rules.Config;
using Microsoft.Extensions.Logging;

namespace CTA.FeatureDetection.Common.Reporting
{
    public class FeatureReportGenerator
    {
        private const string DefaultFeatureReportFilename = "DetectedFeaturesReport.csv";

        /// <summary>
        /// Creates a csv report in string form from a collection of features detected.
        /// </summary>
        /// <param name="featureDetectionResults">Feature Detection results for a set of projects</param>
        /// <param name="loadedFeatures">Features loaded into the feature detector used to create featureDetectionResults</param>
        /// <returns></returns>
        public static string GenerateCsvReport(
            IDictionary<string, FeatureDetectionResult> featureDetectionResults, 
            FeatureSet loadedFeatures)
        {
            if (featureDetectionResults == null || loadedFeatures == null)
            {
                return string.Empty;
            }

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
        /// <param name="csvReport">Report content to save</param>
        /// <param name="directory">Destination directory</param>
        /// <param name="filename">Name of file to write to</param>
        public static void ExportReport(string csvReport, string directory = null, string filename = null)
        {
            directory ??= string.Empty;
            filename = string.IsNullOrEmpty(filename) ? DefaultFeatureReportFilename : filename;
            var exportPath = Path.Combine(directory, filename);

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

        private static IEnumerable<FeatureReportRecord> ConvertFeatureResultsToRecords(
            string projectName,
            FeatureDetectionResult featureDetectionResult,
            IDictionary<string, Feature> featureLookup)
        {
            var featureReportRecords = new List<FeatureReportRecord>();
            foreach (var feature in featureDetectionResult.PresentFeatures)
            {
                try
                {
                    var featureMetadata = featureLookup[feature];
                    var newRecord = new FeatureReportRecord
                    {
                        ProjectName = projectName,
                        FeatureCategory = featureMetadata.FeatureCategory,
                        FeatureName = featureMetadata.Name,
                        Description = featureMetadata.Description,
                        IsLinuxCompatible = featureMetadata.IsLinuxCompatible
                    };
                    featureReportRecords.Add(newRecord);
                }
                catch (KeyNotFoundException e)
                {
                    Log.Logger.LogError(e.ToString());
                }
            }

            return featureReportRecords;
        }
    }
}
