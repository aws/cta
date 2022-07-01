using System.Collections.Generic;
using System.Linq;
using Codelyzer.Analysis;
using Codelyzer.Analysis.Model;
using CTA.Rules.Models;

namespace CTA.Rules.PortCore;

public class PortCoreUtils
{
    public static HashSet<string> GetReferencesForProject(PortCoreConfiguration projectConfiguration, AnalyzerResult analyzerResult)
    {
        var allReferences = new HashSet<string>();
        var projectResult = analyzerResult.ProjectResult;

        projectResult?.SourceFileResults?.SelectMany(s => s.References)?.Select(r => r.Namespace).Distinct().ToList().ForEach(currentReference=> { 
            if (currentReference != null && !allReferences.Contains(currentReference))
            {
                allReferences.Add(currentReference);
            }
        });

        projectResult?.SourceFileResults?.SelectMany(s => s.Children.OfType<UsingDirective>())?.Select(u=>u.Identifier).Distinct().ToList().ForEach(currentReference => {
            if (currentReference != null && !allReferences.Contains(currentReference))
            {
                allReferences.Add(currentReference);
            }
        });
                    
        projectResult?.SourceFileResults?.SelectMany(s => s.Children.OfType<ImportsStatement>())?.Select(u => u.Identifier).Distinct().ToList().ForEach(currentReference =>
        {
            if (currentReference != null && !allReferences.Contains(currentReference))
            {
                allReferences.Add(currentReference);
            }
        });

        return allReferences;
    }
}
