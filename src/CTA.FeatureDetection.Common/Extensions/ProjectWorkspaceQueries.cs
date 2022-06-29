﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codelyzer.Analysis.Model;

namespace CTA.FeatureDetection.Common.Extensions
{
    public static class ProjectWorkspaceQueries
    {
        /// <summary>
        /// Determines if a ProjectWorkspace has a specified Nuget Dependency
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="nugetReferenceIdentifier">Nuget reference to search for</param>
        /// <returns>Whether or not the nuget reference exists in the project</returns>
        public static bool ContainsNugetDependency(this ProjectWorkspace project, string nugetReferenceIdentifier)
            => project.ExternalReferences?.NugetReferences
                   .Any(r => r.Identity == nugetReferenceIdentifier) == true;

        /// <summary>
        /// Determines if a ProjectWorkspace has a specified Dependency
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="referenceIdentifier">Reference to search for</param>
        /// <returns>Whether or not the reference exists in the project</returns>
        public static bool ContainsDependency(this ProjectWorkspace project, string referenceIdentifier)
            => project.ExternalReferences?.NugetReferences
                .Union(project.ExternalReferences?.NugetDependencies)
                .Union(project.ExternalReferences?.SdkReferences)
                   .Any(r => r.Identity == referenceIdentifier) == true;

        /// <summary>
        /// Determines if a ProjectWorkspace declares a class with a specified base type
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="typeOriginalDefinition">Original Definition of the base type being searched for</param>
        /// <returns>Whether or not a class with the specified base type is declared in the project</returns>
        public static bool DeclaresClassWithBaseType(this ProjectWorkspace project, string typeOriginalDefinition)
            => project.SourceFileResults
                .SelectMany(n => n.AllClasses())
                .Any(c => c.BaseTypeOriginalDefinition == typeOriginalDefinition);

        /// <summary>
        /// Determines if a ProjectWorkspace declares a class with a specified base type for vb sln
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="typeOriginalDefinition">Original Definition of the base type being searched for</param>
        /// <returns>Whether or not a class with the specified base type is declared in the project</returns>
        public static bool DeclaresClassBlocksWithBaseType(this ProjectWorkspace project, string typeOriginalDefinition)
            => project.SourceFileResults
                .SelectMany(n => n.AllClassBlocks())
                .Any(c => c.BaseTypeOriginalDefinition == typeOriginalDefinition);


        /// <summary>
        /// Gets all class declaration nodes in a ProjectWorkspace
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <returns>Collection of class declaration nodes in the project with the specified base type</returns>
        public static IEnumerable<ClassDeclaration> GetAllClassDeclarations(this ProjectWorkspace project)
            => project.SourceFileResults.SelectMany(r => r.AllClasses());

        /// <summary>
        /// Determines if a specified directory exists in the project directory and is non-empty
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="directoryName">Name of directory to search for</param>
        /// <param name="searchSubdirectories">Whether or not to search recursively through directories</param>
        /// <returns>Whether or not the specified directory exists in the project directory and is not empty</returns>
        public static bool ContainsNonEmptyDirectory(this ProjectWorkspace project, string directoryName,
            bool searchSubdirectories = true)
        {
            var projectDirectory = project.ProjectRootPath;
            var searchOption = searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var directories = Directory.EnumerateDirectories(projectDirectory, directoryName, searchOption);

            return directories.Any(d => Directory.EnumerateFiles(d, "*", searchOption).Any());
        }

        /// <summary>
        /// Gets all class declaration nodes in a ProjectWorkspace derived from a specified base type
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="baseTypeOriginalDefinition">Base type to search against</param>
        /// <returns>Collection of class declaration nodes in the project with the specified base type</returns>
        public static IEnumerable<ClassDeclaration> GetClassDeclarationsByBaseType(this ProjectWorkspace project,
            string baseTypeOriginalDefinition)
            => project.GetAllClassDeclarations().Where(c => c.HasBaseType(baseTypeOriginalDefinition));

        /// <summary>
        /// Gets all interface declaration nodes in a ProjectWorkspace
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <returns>Collection of interface declaration nodes in the project</returns>
        public static IEnumerable<InterfaceDeclaration> GetAllInterfaceDeclarations(this ProjectWorkspace project)
            => project.SourceFileResults.SelectMany(r => r.AllInterfaces());

        /// <summary>
        /// Gets all Invocation Expressions with the given method name.
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="methodName">Method name to search for</param>
        /// <returns>Collection of invocation expressions with given method name</returns>
        public static IEnumerable<InvocationExpression> GetInvocationExpressionsByMethodName(this ProjectWorkspace project, string methodName)
            => project.SourceFileResults.SelectMany(r => r.AllInvocationExpressions().Where(i => i.MethodName == methodName));

        /// <summary>
        /// Get all object creation expressions in project with the given Semantic Class Type
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="semanticClassType">Semantic Class Type to search for</param>
        /// <returns></returns>
        public static IEnumerable<ObjectCreationExpression> GetObjectCreationExpressionBySemanticClassType(this ProjectWorkspace project, string semanticClassType)
            => project.SourceFileResults.SelectMany(r => r.AllObjectCreationExpressions().Where(o => o.SemanticClassType == semanticClassType));

        /// <summary>
        /// Determines if the project contains a file with the given extension.
        /// </summary>
        /// <param name="project">ProjectWorkspace to search</param>
        /// <param name="extension">Extension name to search for without '.'</param>
        /// <param name="searchSubdirectories">Whether or not to search recursively through directories</param>
        /// <returns>Whether or not a file with extesnion exists in the project directory</returns>
        /// <returns></returns>
        public static bool ContainsFileWithExtension(this ProjectWorkspace project, string extension,
            bool searchSubdirectories = true)
        {
            var projectDirectory = project.ProjectRootPath;
            var searchOption = searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var searchPattern = string.Join(".", "*", extension);

            return Directory.EnumerateFiles(projectDirectory, searchPattern, searchOption).Any();
        }
    }
}
