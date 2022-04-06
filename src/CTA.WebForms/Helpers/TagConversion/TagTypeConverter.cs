using System;
using CTA.WebForms.Helpers.ControlHelpers;

namespace CTA.WebForms.Helpers.TagConversion
{
    /// <summary>
    /// Converts source tag attribute values to proper representation for use
    /// on equivalent attribute of target tag type.
    /// </summary>
    public static class TagTypeConverter
    {
        /// <summary>
        /// Converts a given source attribute to the specified
        /// target type.
        /// </summary>
        /// <param name="sourceAttribute">The name of the source attribute being converted.</param>
        /// <param name="sourceValue">The value of the source attribute being converted.</param>
        /// <param name="targetAttribute">The name of the attribute that takes the converted value,
        /// if one exists.</param>
        /// <param name="targetType">The target type that the source attribute value will be converted to.</param>
        /// <returns>The proper representation of <paramref name="sourceValue"/> as the type <paramref name="targetType"/>.</returns>
        public static string ConvertToType(
            string sourceAttribute,
            string sourceValue,
            string targetAttribute = null,
            AttributeTargetTypes targetType = AttributeTargetTypes.String)
        {
            if (sourceValue == null)
            {
                return string.Empty;
            }

            var embeddingConverted = EmbeddedCodeReplacers.ReplaceOneWayDataBinds(sourceValue);
            embeddingConverted = EmbeddedCodeReplacers.ReplaceRawExprs(embeddingConverted);
            embeddingConverted = EmbeddedCodeReplacers.ReplaceHTMLEncodedExprs(embeddingConverted);

            // If the value is some form of embedding syntax then we want to use the converted
            // embedding instead of using a type conversion, so if our attempts to convert embedding
            // syntaxes succeed then we return that result
            if (!embeddingConverted.Equals(sourceValue))
            {
                return FormatConvertedAttribute(targetAttribute, embeddingConverted);
            }

            return targetType switch
            {
                AttributeTargetTypes.HtmlBoolean => ConvertToHtmlBoolean(sourceAttribute, sourceValue, targetAttribute),
                AttributeTargetTypes.ComponentBoolean => ConvertToComponentBoolean(sourceAttribute, sourceValue, targetAttribute),
                AttributeTargetTypes.InvertedHtmlBoolean => ConvertToHtmlBoolean(sourceAttribute, sourceValue, targetAttribute, true),
                AttributeTargetTypes.InvertedComponentBoolean => ConvertToComponentBoolean(sourceAttribute, sourceValue, targetAttribute, true),
                AttributeTargetTypes.EventHandler => ConvertToEventHandler(sourceValue, targetAttribute),
                // default is same as String and EventCallback (no modification required)
                _ => FormatConvertedAttribute(targetAttribute, sourceValue)
            };
        }

        /// <inheritdoc cref="ConvertToType(string, string, string, AttributeTargetTypes)"/>
        public static string ConvertToType(
            string sourceAttribute,
            string sourceValue,
            string targetAttribute = null,
            string targetType = "String")
        {
            if (sourceValue == null)
            {
                return string.Empty;
            }

            var targetTypeEnum = (AttributeTargetTypes)Enum.Parse(typeof(AttributeTargetTypes), targetType);

            return ConvertToType(sourceAttribute, sourceValue, targetAttribute, targetTypeEnum);
        }

        /// <summary>
        /// Converts a given source attribute to an html boolean representation.
        /// </summary>
        /// <param name="sourceAttribute">The name of the source attribute being converted.</param>
        /// <param name="sourceValue">The value of the source attribute being converted.</param>
        /// <param name="targetAttribute">The name of the attribute that takes the converted value,
        /// if one exists.</param>
        /// <param name="inverted">Whether or not this conversion should invert the result.</param>
        /// <returns>The <paramref name="sourceValue"/> as an html boolean.</returns>
        private static string ConvertToHtmlBoolean(string sourceAttribute, string sourceValue, string targetAttribute, bool inverted = false)
        {
            var isTrue = sourceValue.Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase)
                // For an html boolean <attribute>=<attribute> means true
                || sourceValue.Equals(sourceAttribute, StringComparison.InvariantCultureIgnoreCase)
                // For an html boolean <attribute>, <attribute>="", and <attribute>='' all mean true
                || sourceValue.Equals(string.Empty);

            if ((!inverted && isTrue) || (inverted && !isTrue))
            {
                // For an html boolean, just including the attribute name with no value is the
                // easiest way to set it to true
                // Without a target attribute to set to this value, we return an empty string
                // because this representation only makes sense for attribute assignment
                return targetAttribute == null ? string.Empty : targetAttribute;
            }

            // For an html boolean, excluding the attribute altogether is the easiest way to
            // set it to false
            return string.Empty;
        }

        /// <summary>
        /// Converts a given source attribute to a component boolean representation.
        /// </summary>
        /// <param name="sourceAttribute">The name of the source attribute being converted.</param>
        /// <param name="sourceValue">The value of the source attribute being converted.</param>
        /// <param name="targetAttribute">The name of the attribute that takes the converted value,
        /// if one exists.</param>
        /// <param name="inverted">Whether or not this conversion should invert the result.</param>
        /// <returns>The <paramref name="sourceValue"/> as a component boolean.</returns>
        private static string ConvertToComponentBoolean(string sourceAttribute, string sourceValue, string targetAttribute, bool inverted = false)
        {
            var result = false.ToString();
            var isTrue = sourceValue.Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase)
                // For an html boolean <attribute>=<attribute> means true
                || sourceValue.Equals(sourceAttribute, StringComparison.InvariantCultureIgnoreCase)
                // For an html boolean <attribute>, <attribute>="", and <attribute>='' all mean true
                || sourceValue.Equals(string.Empty);

            if ((!inverted && isTrue) || (inverted && !isTrue))
            {
                result = true.ToString();
            }

            return FormatConvertedAttribute(targetAttribute, result);
        }

        /// <summary>
        /// Converts a given source attribute to an event handler representation. For event
        /// callbacks (which only take event args parameter or no parameters) no modification
        /// should be necessary.
        /// </summary>
        /// <param name="sourceValue">The value of the source attribute being converted.</param>
        /// <param name="targetAttribute">The name of the attribute that takes the converted value,
        /// if one exists.</param>
        /// <returns>The <paramref name="sourceValue"/> as an event handler.</returns>
        private static string ConvertToEventHandler(string sourceValue, string targetAttribute = null)
        {
            // Substituting a null here for sender parameter due to lack of suitable replacements
            var result = $"(args) => {sourceValue}(null, args)";

            return FormatConvertedAttribute(targetAttribute, result);
        }

        /// <summary>
        /// Formats converted result into new attribute or plain text based on value
        /// of <paramref name="attribute"/>. Usable only for basic conversions, target
        /// types with unique representations will have to format their own results.
        /// </summary>
        /// <param name="value">The converted value.</param>
        /// <param name="attribute">The attribute to set the converted value to, if
        /// it exists.</param>
        /// <returns>The formatted attribute conversion result.</returns>
        private static string FormatConvertedAttribute(string attribute, string value)
        {
            if (attribute == null)
            {
                return value;
            }
            else
            {
                value = value.Replace('\"', '\'');
                return $"{attribute}=\"{value}\"";
            }
        }
    }
}
