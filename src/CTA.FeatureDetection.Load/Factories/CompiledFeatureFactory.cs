using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using CTA.FeatureDetection.Common.Exceptions;
using CTA.FeatureDetection.Common.Models.Enums;
using CTA.FeatureDetection.Common.Models.Configuration;
using CTA.FeatureDetection.Common.Models.Features.Base;

[assembly: InternalsVisibleTo("CTA.FeatureDetection.Tests")]
namespace CTA.FeatureDetection.Load.Factories
{
    internal class CompiledFeatureFactory
    {
        /// <summary>
        /// Factory method that dynamically instantiates feature classes using assembly and class metadata
        /// </summary>
        /// <param name="featureScope">The scope in which to look for this feature</param>
        /// <param name="assembly">Assembly containing the feature type</param>
        /// <param name="namespace">Namespace containing the feature type</param>
        /// <param name="featureMetadata">Metadata about the feature type</param>
        /// <returns>Instance of the specified feature</returns>
        public static CompiledFeature GetInstance(FeatureScope featureScope, Assembly assembly, string @namespace, CompiledFeatureMetadata featureMetadata)
        {
            var fullyQualifiedClassName = $"{@namespace}.{featureMetadata.ClassName}";
            var featureType = assembly.GetType(fullyQualifiedClassName);
            if (featureType == null)
            {
                throw new ClassNotFoundException(assembly, fullyQualifiedClassName);
            }

            try
            {
                var name = featureMetadata.Name;
                var scope = featureScope;
                var featureInstance = GetInstance(featureType, name, scope);
                return featureInstance;
            }
            catch (InvalidFeatureException)
            {
                throw new InvalidFeatureException(assembly, fullyQualifiedClassName);
            }
        }

        /// <summary>
        /// Factory method that dynamically instantiates a feature object by its type
        /// </summary>
        /// <param name="featureType">Type of feature to instantiate</param>
        /// <param name="name">Name of feature</param>
        /// <param name="featureScope">The scope in which to look for this feature</param>
        /// <returns>Feature instance</returns>
        public static CompiledFeature GetInstance(Type featureType, string name, FeatureScope featureScope)
        {
            var featureInstance = Activator.CreateInstance(featureType) as CompiledFeature;
            if (featureInstance == null)
            {
                throw new InvalidFeatureException(featureType);
            }

            featureInstance.Name = name;
            featureInstance.FeatureScope = featureScope;

            return featureInstance;
        }

        /// <summary>
        /// Factory method that dynamically instantiates a feature object by its type
        /// </summary>
        /// <param name="featureType">Type of feature to instantiate</param>
        /// <returns>Feature instance</returns>
        public static CompiledFeature GetInstance(Type featureType)
        {
            var featureInstance = Activator.CreateInstance(featureType) as CompiledFeature;
            if (featureInstance == null)
            {
                throw new InvalidFeatureException(featureType);
            }

            return featureInstance;
        }
    }
}