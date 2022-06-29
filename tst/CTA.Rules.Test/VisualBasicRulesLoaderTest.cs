using System.IO;
using System.Reflection;
using CTA.Rules.Config;
using CTA.Rules.RuleFiles;
using NUnit.Framework;
using Newtonsoft.Json;

namespace CTA.Rules.Test;

public class VisualBasicRulesLoaderTest : AwsRulesBaseTest
{
    [Test]
    public void TestRulesFileParser()
    {
        var ctaFilesDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location),
            "CTAFiles"));
        var rootObject = JsonConvert.DeserializeObject<Rootobject>(
            File.ReadAllText(Path.Combine(ctaFilesDir,
                "consolidated.json")));

        var overrideObject = JsonConvert.DeserializeObject<Rootobject>(
            File.ReadAllText(Path.Combine(ctaFilesDir, "project.specific.json")));

        var namespaceRecommendations = JsonConvert.DeserializeObject<NamespaceRecommendations>(
            File.ReadAllText(Path.Combine(ctaFilesDir,
                "vb.rules.test.json")));

        var fileParser = new VisualBasicRulesFileParser(
            namespaceRecommendations,
            new NamespaceRecommendations(), 
            rootObject, 
            overrideObject,
            "",
            "netcoreapp3.1");
        var nodes = fileParser.Process();
        Assert.IsNotNull(nodes);
    }
}