using System;

namespace CTA.WebForms.Helpers.TagConversion
{
    /// <summary>
    /// Custom exception class to be used when some form of invalid configuration
    /// is discovered while validating models deserialized from tag configs.
    /// </summary>
    [Serializable]
    public class ConfigValidationException : Exception
    {
        public ConfigValidationException() { }
        public ConfigValidationException(string message) : base(message) { }
        public ConfigValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
