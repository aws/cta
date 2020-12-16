
using System.Collections.Generic;

public class NamespaceRecommendations
{
    public NamespaceRecommendations()
    {
        NameSpaces = new List<Namespaces>();
    }
    public List<Namespaces> NameSpaces { get; set; }
}

public class Namespaces
{
    public Namespaces()
    {
        Recommendations = new List<Recommendations>();
    }
    public string Name { get; set; }
    public string Version { get; set; }
    public List<Packages> Packages { get; set; }
    public List<Recommendations> Recommendations { get; set; }
}
public class Packages
{
    public string Name { get; set; }
    public string Type { get; set; }
}
public class Recommendations
{
    public Recommendations()
    {
        RecommendedActions = new List<RecommendedActions>();
    }
    public string Type { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string KeyType { get; set; }
    public string ContainingType { get; set; }
    public List<RecommendedActions> RecommendedActions { get; set; }
}

public class RecommendedActions
{
    public RecommendedActions()
    {
        TargetFrameworks = new List<TargetFramework>();
        Actions = new List<Action>();
    }
    public string Source { get; set; }
    public string Preferred { get; set; }
    public List<TargetFramework> TargetFrameworks { get; set; }
    public string Description { get; set; }
    public List<Action> Actions { get; set; }
}

public class TargetFramework
{
    public TargetFramework()
    {
        TargetCPU = new List<string>();
    }
    public string Name { get; set; }
    public List<string> TargetCPU { get; set; }
}
