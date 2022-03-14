using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTA.Rules.Config;
using CTA.WebForms.TagConverters;
using YamlDotNet.Serialization;

namespace CTA.WebForms.Helpers.TagConversion
{
    /// <summary>
    /// Responsible for reading downloaded tag config files and parsing them
    /// into types derived from <see cref="TagConverter"/>.
    /// </summary>
    public class TagConfigParser
    {
        private readonly string _configsDir;
        private readonly IDeserializer _deserializer;

        /// <summary>
        /// Initializes a new instance of <see cref="TagConfigParser"/> for reading
        /// from a specified location.
        /// </summary>
        /// <param name="configsDir">The directory to read tag config files from.</param>
        public TagConfigParser(string configsDir)
        {
            _configsDir = configsDir;
            _deserializer = GenerateDeserializer();
        }

        /// <summary>
        /// Generates a dictionary mapping tag name to corresponding <see cref="TagConverter"/> from
        /// discovered tag config files.
        /// </summary>
        /// <returns>A dictionary mapping tag name to corresponding <see cref="TagConverter"/>.</returns>
        public IDictionary<string, TagConverter> GetConfigMap()
        {
            var result = new Dictionary<string, TagConverter>();
            var filePaths = Directory.EnumerateFiles(_configsDir, "*.yaml");

            foreach (var filePath in filePaths)
            {
                try
                {
                    var tagName = GetTagNameForFile(filePath);
                    var converter = GetConverterForFile(filePath);

                    if (converter.Validate())
                    {
                        result.Add(tagName, converter);
                    }
                    else
                    {
                        LogHelper.LogError($"{Rules.Config.Constants.WebFormsErrorTag}Failed to validate generated converter for config at {filePath}");
                    }
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to parse tag config at {filePath}");
                }
            }

            return result;
        }

        /// <summary>
        /// Uses the path to a tag config file to generate the tag name that it
        /// corresponds to.
        /// </summary>
        /// <param name="filePath">The file path of the tag config file.</param>
        /// <returns>The tag name for the specified file.</returns>
        /// <exception cref="ArgumentException">Throws if the file name does not follow the
        /// tagPrefix.tagName.yaml naming convention.</exception>
        private static string GetTagNameForFile(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath ?? string.Empty);

            var nameComponents = fileName.Split(".");

            if (nameComponents.Length != 2)
            {
                throw new ArgumentException($"File at {filePath} does not follow correct naming convention (tagPrefix.tagName.yaml)");
            }

            return $"{nameComponents[0]}:{nameComponents[1]}";
        }

        /// <summary>
        /// Uses a yaml deserializer to generate corresponding <see cref="TagConverter"/> from the
        /// contents of a tag config file.
        /// </summary>
        /// <param name="filePath">The file path of the tag config file.</param>
        /// <returns>The corresponding <see cref="TagConverter"/> for the tag config file.</returns>
        /// <exception cref="ArgumentException">Throws if file at <paramref name="filePath"/> is
        /// not a valid tag config.</exception>
        private TagConverter GetConverterForFile(string filePath)
        {
            TagConverter result = null;

            using (var reader = new StreamReader(filePath))
            {
                result = _deserializer.Deserialize(reader) as TagConverter;
            }

            if (result == null)
            {
                throw new ArgumentException($"File at {filePath} is not a valid tag config");
            }

            return result;
        }

        /// <summary>
        /// Generates a yaml deserializer configured for <see cref="TagConverter"/> deserialization.
        /// </summary>
        /// <returns>A yaml deserializer configured for <see cref="TagConverter"/> deserialization.</returns>
        private static IDeserializer GenerateDeserializer()
        {
            var webFormsAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.FullName.StartsWith("CTA.WebForms") && !a.FullName.Contains("Test"))
                .FirstOrDefault();
            var types = webFormsAssembly.GetTypes();
            
            var converterTypes = types.Where(t => t.Name.EndsWith(Constants.TagConverterSuffix) && !t.IsAbstract && !t.IsInterface);
            var templateConditionTypes = types.Where(t => t.Name.EndsWith(Constants.TemplateConditionSuffix) && !t.IsAbstract && !t.IsInterface);
            var templateInvokableTypes = types.Where(t => t.Name.EndsWith(Constants.TemplateInvokableSuffix) && !t.IsAbstract && !t.IsInterface);

            var deserializerBuilder = new DeserializerBuilder();
            deserializerBuilder = AddTagMappings(deserializerBuilder, converterTypes, Constants.TagConverterSuffix);
            deserializerBuilder = AddTagMappings(deserializerBuilder, templateConditionTypes, Constants.TemplateConditionSuffix);
            deserializerBuilder = AddTagMappings(deserializerBuilder, templateInvokableTypes, Constants.TemplateInvokableSuffix);

            return deserializerBuilder.Build();
        }

        /// <summary>
        /// Used to add tag mappings to a <see cref="DeserializerBuilder"/> for a group
        /// of types which share a suffix.
        /// </summary>
        /// <param name="deserializerBuilder">The <see cref="DeserializerBuilder"/> to modify.</param>
        /// <param name="types">The group of types to be mapped.</param>
        /// <param name="suffix">The suffix that the types share.</param>
        /// <returns>The modified <see cref="DeserializerBuilder"/>.</returns>
        private static DeserializerBuilder AddTagMappings(
            DeserializerBuilder deserializerBuilder,
            IEnumerable<Type> types,
            string suffix)
        {
            foreach (var type in types)
            {
                var typeName = type.Name;
                var yamlTag = typeName.Substring(0, typeName.Length - suffix.Length);
                deserializerBuilder = deserializerBuilder.WithTagMapping($"!{yamlTag}", type);
            }

            return deserializerBuilder;
        }
    }
}
