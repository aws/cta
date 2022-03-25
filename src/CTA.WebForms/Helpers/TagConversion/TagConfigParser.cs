using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        /// The map of known tag names to converter instances.
        /// </summary>
        public ConcurrentDictionary<string, TagConverter> TagConverterMap { get; }
        /// <summary>
        /// The list of known tag names which do not have converters. We use a 
        /// <see cref="ConcurrentBag{string}"/> here because there is no concurrent/
        /// thread safe Set class, and potentially having duplicates in the bag doesn't
        /// cause any issues for us.
        /// </summary>
        public ConcurrentBag<string> NoConverterTags { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="TagConfigParser"/> for reading
        /// from a specified location.
        /// </summary>
        /// <param name="configsDir">The directory to read tag config files from.</param>
        public TagConfigParser(string configsDir)
        {
            _configsDir = configsDir;
            _deserializer = GenerateDeserializer();

            TagConverterMap = new ConcurrentDictionary<string, TagConverter>(StringComparer.InvariantCultureIgnoreCase);
            NoConverterTags = new ConcurrentBag<string>();
        }

        /// <summary>
        /// Gets the tag converter that should be used on a given node.
        /// </summary>
        /// <param name="nodeName">The node to be converted.</param>
        /// <returns>The converter to be used on nodes of type <paramref name="nodeName"/>, null
        /// if no such converter exists.</returns>
        public TagConverter GetConfigForNode(string nodeName)
        {
            if (NoConverterTags.Contains(nodeName, StringComparer.InvariantCultureIgnoreCase))
            {
                return null;
            }

            if (TagConverterMap.ContainsKey(nodeName))
            {
                return TagConverterMap[nodeName];
            }

            string fileName = GetValidFileName(nodeName);
            string filePath = Path.Combine(_configsDir, fileName);
            string s3Url = $"{Rules.Config.Constants.S3TagConfigsBucketUrl}\\{fileName}";

            if (File.Exists(filePath) || DownloadFileFromS3(s3Url, filePath))
            {
                try
                {
                    var converter = GetConverterForFile(filePath);

                    // If validation fails, an exception will be thrown and we will enter the catch
                    // block before we attempt to add the converter to the results dictionary
                    converter.Validate();

                    if (!TagConverterMap.TryAdd(converter.TagName, converter))
                    {
                        LogHelper.LogError($"{Rules.Config.Constants.WebFormsErrorTag}Failed to add valid converter of type " +
                            $"{converter?.GetType().Name} for {converter?.TagName} to concurrent dictionary for config at {filePath} " +
                            $"due to attempted invalid concurrent access");
                    }

                    // Even if adding to the dictionary fails, we want to return the validated converter,
                    // the dictionary only serves to improve performance when retrieving converters
                    return converter;
                }
                catch (Exception e)
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to parse tag config at {filePath}");
                }
            }

            NoConverterTags.Add(nodeName);
            return null;
        }

        /// <summary>
        /// Constructs a valid file name using the given node name by stripping out invalid
        /// characters and switching : charcters to . characters.
        /// </summary>
        /// <param name="nodeName">The node name to construct the file name from.</param>
        /// <returns>The new file name.</returns>
        private string GetValidFileName(string nodeName)
        {
            string fileName = $"{nodeName.Replace(":", ".")}.yaml";

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), string.Empty);
            }

            return fileName.ToLower();
        }

        /// <summary>
        /// Attempts to download a config file at the given url to the given file path.
        /// </summary>
        /// <param name="s3Url">The url to fetch the file from.</param>
        /// <param name="filePath">The file path to save the file at should it be found.</param>
        /// <returns><c>true</c> if the file was retrieved and written to the file system
        /// successfully, <c>false</c> otherwise.</returns>
        private bool DownloadFileFromS3(string s3Url, string filePath)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var fileContents = httpClient.GetStringAsync(s3Url).Result;
                    File.WriteAllText(filePath, fileContents);
                }

                return true;
            }
            catch (Exception e)
            {
                // 404 is not an error, we just don't have a config for this node type. Other
                // exceptions, however, are "real" errors
                if (!e.Message.Contains("404"))
                {
                    LogHelper.LogError(e, $"{Rules.Config.Constants.WebFormsErrorTag}Failed to download and " +
                        $"store tag config file from {s3Url} to {filePath}");
                }
                
                return false;
            }
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
