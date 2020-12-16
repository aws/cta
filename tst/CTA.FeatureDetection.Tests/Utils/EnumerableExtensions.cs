using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CTA.FeatureDetection.Tests.Utils
{
    public static class Extensions
    {
        public static bool IsCompilerGenerated(this Type type)
        {
            var attr = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).FirstOrDefault();
            return attr != null;
        }
    }
}