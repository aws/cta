using CTA.Rules.Config;
using Microsoft.Extensions.Logging;

namespace CTA.FeatureDetection.Common
{
    public static class Log
    {
        /// <summary>
        /// Get global logger instance or initialize a logger w/ a default config if it's null
        /// </summary>
        /// <returns>Global logger instance</returns>
        public static ILogger Logger
        {
            get => LogHelper.Logger;
        }

    }
}