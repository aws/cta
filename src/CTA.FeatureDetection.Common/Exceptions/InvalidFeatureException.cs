using System;
using System.Reflection;

namespace CTA.FeatureDetection.Common.Exceptions
{
    public class InvalidFeatureException : Exception
    {
        public InvalidFeatureException(Assembly assembly, string className) 
            : base(DefaultMessage(assembly, className))
        {
        }

        public InvalidFeatureException(Type type) 
            : base(DefaultMessage(type))
        {
        }

        private static string DefaultMessage(Assembly assembly, string className)
        {
            return $"The class {className} in assembly {assembly.FullName} is not a valid Feature.";
        }

        private static string DefaultMessage(Type type)
        {
            return $"The class {type} is not a valid Feature.";
        }
    }
}