# Code translation in Porting Assistant for .NET
![Build Test](https://github.com/aws/cta/workflows/Build%20Test/badge.svg)

Porting Assistant for .NET is an analysis tool that scans .NET Framework applications and generates a .NET Core compatibility assessment, helping customers port their applications to Linux faster.

Porting Assistant for .NET scans .NET Framework applications to identify incompatibilities with .NET Core, finds known replacements, generates detailed compatibility assessment reports and makes changes to the source code to fix common issues encountered while porting an application from .NET Framework to .NET Core. This reduces the manual effort involved in modernizing applications to Linux.

The code translation package contains the source of the engine that allow developers to define recommendations to fix incompatible patterns in the source code when porting from .NET Framework to .NET Core or .NET 5.

For more information about Porting Assistant and to try the tool, please refer to the documentation: https://aws.amazon.com/porting-assistant-dotnet/

# Introduction

Code translation feature helps users automate some of their porting experience when converting their applications from .NET Framework to .NET Core. It identifies common issues such as usage of Entity Framework or ASP.NET MVC and makes changes to the source code to reduce the amount of work needed to port applications from .NET Framework to .NET Core or .NET 5. 

In order to perform code translations, code translation relies on a set of predefined rules and actions. The rules files define patterns that the code translation package searches for in the user code, the tool then performs actions specified in the rules files to fix the issues that were identified. Below we have highlighted some examples of rules that are already available. 

#### ASP .NET MVC:
If you’re porting your MVC project to .NET Core or .NET 5, below are some of the steps that Porting Assistant will do:

* Creates a new project file that uses the SDK for Web projects
* Update the namespaces to use the new Microsoft.AspNetCore.Mvc
* Update the project structure and move your static files (CSS, JavaScript, images, etc.) into the static files folder
* Add the required template files to start your application (Program.cs and Startup.cs)
* Archive framework files that are no longer needed (global.asax, BundleConfig.cs, etc.)

#### Database Connectivity:
Whether you’re using ADO .NET or Entity Framework, Porting Assistant for .NET can help you with the porting process.

ADO .NET:
Porting Assistant for .NET will add the needed packages to the csproj files. In addition, it will scan your code and automatically update any references to the framework ADO .NET namespace, System.Data.SqlClient, to the new .NET core or .NET 5 compatible package, Microsoft.Data.SqlClient

Entity Framework:
Users who have dependencies on Entity Framework in their projects will also see their porting experience enhanced. During porting, Porting assistant will automatically:

* Add the NuGet packages for EF Core
* Update the namespaces in your code files
* Add an OnConfiguring method to your DbContext classes to allow you to easily connect to your database

In some cases, and depending where you store your connection strings, the porting process will also migrate your connection strings to the new appsettings file in .NET Core.

In addition to the above rules, Porting Assistant will apply other rules depending on your code. We have a list of around 20 rules and 40 actions that are grouped by namespace, and are [open source](https://github.com/aws/porting-assistant-dotnet-datastore). We will continue enhancing and adding to this list, and welcome any contributions from the community.



## Getting Started

* Clone the Git repository.
* Load the solution CTA.Rules.sln using Visual Studio or Rider.
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
  * -c (create-new): Create a new folder to output the ported solution into (folder will be created at the parent folder of the given solution or project file)

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
   "Name": "CreateNet3FolderHierarchy",
   "Type": "Project",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "CreateNet5FolderHierarchy",
   "Type": "Project",
   "Value": "", //Parameter(s) passed to the action
   "Description": "Sample Description for the actions",
   "ActionValidation": {"Contains": "","NotContains": ""}
}
{
   "Name": "MigrateConfig",
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

