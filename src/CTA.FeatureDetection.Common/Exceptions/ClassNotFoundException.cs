using System;
using System.Reflection;

namespace CTA.FeatureDetection.Common.Exceptions
{
    public class ClassNotFoundException : Exception
    {
        public ClassNotFoundException(Assembly assembly, string className)
            : base(DefaultMessage(assembly, className))
        {
        }

        private static string DefaultMessage(Assembly assembly, string className)
        {
            return $"Could not find {className} class in assembly {assembly.FullName}";
        }
    }
}