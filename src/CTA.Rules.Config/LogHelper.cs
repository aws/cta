using System;
using Microsoft.Extensions.Logging;

namespace CTA.Rules.Config
{
    /// <summary>
    /// Class for access to logger throughout the project
    /// </summary>
    public class LogHelper
    {
        private static ILogger _logger;


        public static ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
                    _logger = loggerFactory.CreateLogger(Constants.Translator);
                }
                return _logger;
            }
            set { _logger = value; }
        }


        public static void LogError(string message, params object[] args)
        {
            Logger.LogError(message, args);
        }

        public static void LogError(Exception ex, string message = null, params object[] args)
        {
            Logger.LogError(ex, message, args);
        }

        public static void LogDebug(string message, params object[] args)
        {
            Logger.LogDebug(message, args);
        }
        public static void LogInformation(string message, params object[] args)
        {
            Logger.LogInformation(message, args);
        }
        public static void LogWarning(string message, params object[] args)
        {
            Logger.LogWarning(message, args);
        }
    }
}
