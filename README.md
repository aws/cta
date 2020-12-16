# Code Translation Assistant (CTA)
![Build Test](https://github.com/aws/cta/workflows/Build%20Test/badge.svg)

## Introduction

Code translation assistant is a tool to help users automate some of their porting experience using a set of predefined rules and actions. The rules file is used to find patterns in the user code and compile a list of applicable actions. These actions are later run to modify the code, in addition to making some project changes when applicable. 

## Getting Started

* Clone the Git repository.
* Load the solution Codelyzer.sln using Visual Studio or Rider.
* Create a "Run/Debug" Configuration for the "CTA.Rules.PortCore" project.
    * The project will pre-load rules for porting your framework solution to .net core if you use the argument -d true to load these rules
* Provide command line arguments for a solution and output path, then run the application. List of arguments below:
  * -p (project-path): Path to a project (or solution file)
  * -s (solution-path): Path to a solution file
  * -r (rules-input-file): Path to directory containing custom rule files to be applied
  * -d (use-builtin-rules): Download and apply set of default rules from S3
  * -a (assemblies-dir): Path to assembly containing custom actions specified in your rules
  * -v (version): Version of .net core to port to (currently supports netcoreapp3.1 and net5.0)
  * -m (mock-run): Run the rules without applying changes. Used to generate a run report before applying any changes

## Recommendations

Recommendations files are used to create mappings that can be used when porting a .net framework project to .net core. Each namespace will have its own file, and the data from the namespace file will be used when analyzing the compatibility and possible replacements for artifacts inside that namespace.
The recommendation file will have the following properties:

Property Name|Description
---|---
Name|The namespace this file applies to
Packages|The list of packages that this namespace belongs to
Version|The version of number of this recommendation
Recommendations|The list of recommendations that are applicable under this namespace

The Recommendations section is where we specify the changes needed to convert to .net core. Each recommendation will have the below structure:

Property Name|Description
---|---
Type|The node to look for in the code for this recommendation. This can be one of the following values:<br/>-	Namespace: Looks for a namespace to match the token<br/>-	Class: Looks for a class declaration to match the token<br/>-	Interface: Looks for an interface to match<br/>-	Method: Looks for a method declaration<br/>-	Attribute: Looks for an attribute<br/>-	ObjectCreation: Looks for an object creation expression<br/>-	Project: Used for project level modifications (file changes, csproj modifications, etc…)<br/>Name|The name of the node to look for. For example, if we’re looking for a class, name can be the name of the class. For example, SelectList would be a name of class we look for<br/>
Value|The value of the node to look for. This is usually the name, in addition to the namespace and type. For example, the value of SelectList would be System.Web.Mvc.SelectList
KeyType|This is the type of key we’re looking for. By default, this looks for the name of the node. However, there are other options to find nodes by. Below is a list of these options:<br/>-	Namespace: Name<br/>- Class: BaseClass, ClassName, Identifier<br/>- Attribute: Name<br/>- Method: Name<br/>
ContainingType|This will be the type in which a node resides. This is an optional field that is used to find nodes only within certain objects
RecommendedActions|This section describes the actions needed to migrate the node found using the properties above


The RecommendedAction section has the below properties:
Property Name|Description
---|---
Source|The source of the recommendation. This can be from the open source community (OpenSource) or Amazon (Amazon)
Preferred|If multiple versions of an action apply to node, the first preferred version will be picked
TargetFrameworks|This section describes what version of .net core this recommendation applies to. The supported versions are 3.1 and 5.0
Description|This is the description of the action.
Actions|This node defines what actions are to be taken when a node is matched

## Actions
Property Name |	Description
---|---
Name	| The name of the action
Type	|The type of the action (what node it runs on)
Value	| The parameter(s) passed to the action. It can be a string or a json object
Description	| The description of the action
ActionValidation	| An optional action validation that runs on the file after its completion. The system checks the resulting file for the Contains/NotContains attributes to confirm the success of the action



### Attribute
```javascript
{
   "Name": "ChangeAttribute",
   "Type": "Attribute",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": "Test"}
}
```
### AttributeList
```javascript
{
   "Name": "AddComment",
   "Type": "AttributeList",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### Class
```javascript
{
   "Name": "RemoveBaseClass",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AddBaseClass",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "ChangeName",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "RemoveAttribute",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AddAttribute",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AddComment",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AddMethod",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "RemoveMethod",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "RenameClass",
   "Type": "Class",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### CompilationUnit
```javascript
{
   "Name": "AddDirective",
   "Type": "Using",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "RemoveDirective",
   "Type": "Using",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### IdentifierName
```javascript
{
   "Name": "ReplaceIdentifier",
   "Type": "Identifier",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "ReplaceIdentifierInsideClass",
   "Type": "Identifier",
   "Value": {"identifier": "","ClassFullKey": ""}, //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### Interface
```javascript
{
   "Name": "ChangeName",
   "Type": "Interface",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "RemoveAttribute",
   "Type": "Interface",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AddAttribute",
   "Type": "Interface",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AddComment",
   "Type": "Interface",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AddMethod",
   "Type": "Interface",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "RemoveMethod",
   "Type": "Interface",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### InvocationExpression
```javascript
{
   "Name": "ReplaceMethod",
   "Type": "Method",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AppendMethod",
   "Type": "Method",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "AddComment",
   "Type": "Method",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### Namespace
```javascript
{
   "Name": "RenameNamespace",
   "Type": "Namespace",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### ObjectCreation
```javascript
{
   "Name": "ReplaceObjectInitialization",
   "Type": "ObjectCreation",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### ProjectFile
```javascript
{
   "Name": "MigrateProjectFile",
   "Type": "ProjectFile",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```
### ProjectLevel
```javascript
{
   "Name": "ArchiveFiles",
   "Type": "Project",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "CreateMvc3FolderHierarchy",
   "Type": "Project",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "CreateMvc5FolderHierarchy",
   "Type": "Project",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "MigrateWebConfig",
   "Type": "Project",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
```

# Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

# License

This project is licensed under the Apache-2.0 License.

