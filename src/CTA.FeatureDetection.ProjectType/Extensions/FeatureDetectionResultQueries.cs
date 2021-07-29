using System.Collections.Generic;
using CTA.FeatureDetection.Common.Models;

namespace CTA.FeatureDetection.ProjectType.Extensions
{
    public static class FeatureDetectionResultQueries
    {
        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a .NET Core WebAPI project
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a .NET Core WebAPI project</returns>
        public static bool IsCoreWebApiProject(this FeatureDetectionResult featureDetectionResult)
        {
            return featureDetectionResult.FeatureStatus.GetValueOrDefault(Constants.AspNetCoreWebApiFeatureName, false);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a .NET Core MVC project
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a .NET Core MVC project</returns>
        public static bool IsCoreMvcProject(this FeatureDetectionResult featureDetectionResult)
        {
            return featureDetectionResult.FeatureStatus.GetValueOrDefault(Constants.AspNetCoreMvcFeatureName, false);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a .NET Framework WebAPI project
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a .NET Framework WebAPI project</returns>
        public static bool IsWebApiProject(this FeatureDetectionResult featureDetectionResult)
        {
            return featureDetectionResult.FeatureStatus.GetValueOrDefault(Constants.AspNetWebApiFeatureName, false);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a .NET Framework MVC project
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a .NET Framework MVC project</returns>
        public static bool IsMvcProject(this FeatureDetectionResult featureDetectionResult)
        {
            return featureDetectionResult.FeatureStatus.GetValueOrDefault(Constants.AspNetMvcFeatureName, false);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a WebAPI and MVC project
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a WebAPI and MVC project</returns>
        public static bool IsWebApiAndMvcProject(this FeatureDetectionResult featureDetectionResult)
        {
            return IsWebApiProject(featureDetectionResult) && IsMvcProject(featureDetectionResult);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a WebAPI project only
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a WebAPI project only</returns>
        public static bool IsWebApiProjectOnly(this FeatureDetectionResult featureDetectionResult)
        {
            return IsWebApiProject(featureDetectionResult) && !IsMvcProject(featureDetectionResult);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a Web class library
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a Web class library</returns>
        public static bool IsWebClassLibrary(this FeatureDetectionResult featureDetectionResult)
        {
            return featureDetectionResult.FeatureStatus.GetValueOrDefault(Constants.WebClassLibraryFeatureName, false);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a WCF Config based Service Project.
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a WCF Config Based Service Project</returns>
        public static bool IsWCFServiceConfigBasedProject(this FeatureDetectionResult featureDetectionResult)
        {
            return featureDetectionResult.FeatureStatus.GetValueOrDefault(Constants.WCFServiceConfigFeatureName, false);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a WCF Code based Service Project.
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a WCF Code Based Service Project</returns>
        public static bool IsWCFServiceCodeBasedProject(this FeatureDetectionResult featureDetectionResult)
        {
            return featureDetectionResult.FeatureStatus.GetValueOrDefault(Constants.WCFServiceCodeFeatureName, false);
        }

        /// <summary>
        /// Queries a FeatureDetectionResult object to determine if the project is a WCF Client Project.
        /// </summary>
        /// <param name="featureDetectionResult">Result from feature detection</param>
        /// <returns>Whether or not the project is a WCF Client Project</returns>
        public static bool IsWCFClientProject(this FeatureDetectionResult featureDetectionResult)
        {
            return featureDetectionResult.FeatureStatus.GetValueOrDefault(Constants.WCFClientFeatureName, false);
        }
    }
}
