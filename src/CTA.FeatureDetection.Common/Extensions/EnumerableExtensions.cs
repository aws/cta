using System.Collections.Generic;
using System.Linq;

namespace CTA.FeatureDetection.Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines if an enumerable is null or contains zero items
        /// </summary>
        /// <typeparam name="T">Type of items in collection</typeparam>
        /// <param name="enumerable">Collection being checked</param>
        /// <returns>Whether or not the collection is null or contains zero items</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}
