using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace CTA.FeatureDetection
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            /* Select a test project */
            //var solutionPath = Path.Combine("..", "..", "..", "..", "FeatureDetection.Tests", "Examples", "Projects", "ProjectType_Test", "ProjectType_Test.sln");
            //var solutionPath = Path.Combine("..", "..", "..", "..", "FeatureDetection.Tests", "Examples", "Projects", "EF6_Test", "EF6_Test.sln");
            var solutionPath = Path.Combine("..", "..", "..", "..", "FeatureDetection.Tests", "Examples", "Projects", "Test_Solution_For_FeatureDetection", "Test_Solution_For_FeatureDetection.sln");

            /* Select a feature config file*/
            var featureConfigFilePath = Path.Combine("..", "..", "..", "..", "FeatureDetection.Tests", "Examples", "Input", "feature_config.json");


            // Identify all features in a given project
            var featureDetector = new FeatureDetector(featureConfigFilePath);
            var featureDetectorResults = featureDetector.DetectFeaturesInSolution(solutionPath);

            // Display features found in solution
            foreach (var featureDetectorResult in featureDetectorResults)
            {
                var projectPath = featureDetectorResult.Key;
                var result = featureDetectorResult.Value;

                var featuresFound = result.PresentFeatures.ToList();
                Console.WriteLine();
                Console.WriteLine($"Results for {projectPath}:");
                Console.WriteLine($"Number of features found: {featuresFound.Count}");
                Console.WriteLine("List of features found: ");
                featuresFound.ForEach(Console.WriteLine);
            }

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}