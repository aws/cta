
using System.Collections.Generic;
using CTA.Rules.Models;

public class Rootobject
{
    public Rootobject()
    {
        NameSpaces = new List<Namespace>();
    }
    public List<Namespace> NameSpaces { get; set; }
}
public class Namespace
{
    public Namespace()
    {
        Classes = new List<Class>();
        Interfaces = new List<Interface>();
        Actions = new List<Action>();
    }
    public string @namespace { get; set; }
    public string Assembly { get; set; }
    public string Type { get; set; }
    public string Reference { get; set; }
    public List<Class> Classes { get; set; }
    public List<Interface> Interfaces { get; set; }
    public List<Action> Actions { get; set; }
}

public class @Class
{
    public Class()
    {
        Methods = new List<Method>();
        Actions = new List<Action>();
        Attributes = new List<Attribute>();
        ObjectCreations = new List<ObjectCreation>();
    }
    public string Key { get; set; }
    public string FullKey { get; set; }
    public string KeyType { get; set; }
    public List<Method> Methods { get; set; }
    public List<Action> Actions { get; set; }
    public List<Attribute> Attributes { get; set; }
    public List<ObjectCreation> ObjectCreations { get; set; }
}

public class Interface
{
    public Interface()
    {
        Methods = new List<Method>();
        Actions = new List<Action>();
        Attributes = new List<Attribute>();
    }
    public string Key { get; set; }
    public string FullKey { get; set; }
    public string KeyType { get; set; }
    public List<Method> Methods { get; set; }
    public List<Action> Actions { get; set; }
    public List<Attribute> Attributes { get; set; }
}


public class Method
{
    public Method()
    {
        Actions = new List<Action>();
    }
    public string Key { get; set; }
    public string FullKey { get; set; }
    public List<Action> Actions { get; set; }
}

public class Action
{
    public string Name { get; set; }
    public string Type { get; set; }
    public dynamic Value { get; set; }
    public string Description { get; set; }
    public ActionValidation ActionValidation { get; set; }
}

public class Attribute
{
    public Attribute()
    {
        Actions = new List<Action>();
    }
    public string Key { get; set; }
    public string FullKey { get; set; }
    public List<Action> Actions { get; set; }
}

public class ObjectCreation
{
    public ObjectCreation()
    {
        Actions = new List<Action>();
    }
    public string Key { get; set; }
    public string FullKey { get; set; }
    public List<Action> Actions { get; set; }
}
