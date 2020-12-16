using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Codelyzer.Analysis;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Features.Conditions.Base;
using Microsoft.Extensions.Logging;

namespace CTA.FeatureDetection.Common.Models.Features.Conditions
{
    public class XmlFileQueryCondition: Condition
    {
        private ILogger Logger => Log.Logger;

        public string FileNamePattern { get; set; }
        public string[] FileNamePatterns { get; set; } = {};
        public string XmlElementPath { get; set; }
        public string SearchPath { get; set; }
        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;
        public RegexOptions IgnoreCase { get; set; } = RegexOptions.IgnoreCase;
        public Dictionary<string, object> XmlElementAttributes { get; set; }

        public XmlFileQueryCondition(ConditionMetadata conditionMetadata)
            : base(conditionMetadata)
        {
        }

        public override bool IsConditionMet(AnalyzerResult analyzerResult)
        {
            var directoryToSearch = SearchPath ?? analyzerResult?.ProjectResult.ProjectRootPath ?? string.Empty;
            var xmlFiles = FindFileNamePatternInDirectory(directoryToSearch);

            var filesWithAttributes = xmlFiles
                .Where(xmlFile => GetElementsInPath(XmlElementPath, xmlFile.Value)
                    .Any(element => ContainsAllAttributes(element, XmlElementAttributes)))
                .Select(xmlFile => xmlFile.Key);

            return filesWithAttributes.Any() == MatchType;
        }

        private Dictionary<string, XDocument> FindFileNamePatternInDirectory(string directory)
        {
            var filePatternsToLookFor = FileNamePatterns.ToList();
            if (!string.IsNullOrEmpty(FileNamePattern))
            {
                filePatternsToLookFor.Add(FileNamePattern);
            }

            var filesFound = filePatternsToLookFor.SelectMany(filePattern => 
                Directory.EnumerateFiles(directory, "*", SearchOption)
                    .Where(name => Regex.IsMatch(name, filePattern, IgnoreCase)));

            var filesAsXDocuments = new Dictionary<string, XDocument>();
            foreach (var fileFound in filesFound)
            {
                try
                {
                    filesAsXDocuments[fileFound] = XDocument.Load(fileFound);
                }
                catch (XmlException ex)
                {
                    Logger.LogError(ex, $"Could not parse xml file: {fileFound}.");
                }
            }

            return filesAsXDocuments;
        }

        private IEnumerable<XElement> GetElementsInPath(string xmlElementPath, XDocument xml)
        {
            var x = xml.XPathSelectElements(xmlElementPath);
            return xml.XPathSelectElements(xmlElementPath);
        }

        private bool ContainsAllAttributes(XElement element, Dictionary<string, object> xmlElementAttributes)
        {
            return xmlElementAttributes.All(attribute =>
            {
                var attributeInElement = element.Attribute(attribute.Key);
                if (attributeInElement == null)
                {
                    // TODO: log exception, print entire element
                    return false;
                }

                return attributeInElement.Value == (string) attribute.Value;
            });
        }
    }
}