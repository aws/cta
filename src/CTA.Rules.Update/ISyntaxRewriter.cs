#nullable enable
using System.Collections.Generic;
using CTA.Rules.Models;
using Microsoft.CodeAnalysis;

namespace CTA.Rules.Update;

public interface ISyntaxRewriter
{
    public SyntaxNode? Visit(SyntaxNode? node);
    public List<GenericActionExecution> AllExecutedActions { get; set; }
}
