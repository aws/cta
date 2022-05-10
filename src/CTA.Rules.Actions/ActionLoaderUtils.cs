using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using CTA.Rules.Config;
using Newtonsoft.Json;

namespace CTA.Rules.Actions;

public class ActionLoaderUtils
{
    /// <summary>
    /// Gets the action by invoking the methods that will create it
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <param name="actions">List of actions on the type T</param>
    /// <param name="invokeObject">The object that will be used to retrieve the action</param>
    /// <param name="name">Name of the action</param>
    /// <param name="value">Parameter(s) of the action</param>
    /// <returns></returns>
    public static T GetAction<T>(List<MethodInfo> actions, object invokeObject, string name, dynamic value)
    {
        T val = default;
        try
        {
            string actionName = GetActionName(name);
            var method = actions.Where(m => m.Name == actionName).FirstOrDefault();
            if (method == null)
            {
                LogHelper.LogDebug(string.Format("No such action {0}", actionName));
            }
            else
            {
                var parameters = ActionLoaderUtils.GetParameters(value, method);

                if (parameters != null)
                {
                    val = (T)method.Invoke(invokeObject, parameters);
                }
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError(ex, "Error while loading action {0}", name);
        }
        return val;
    }
    
    public static List<Assembly> GetAssemblies(List<string> assemblyPaths)
    {
        var actionsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.FullName.Contains("CTA.Rules.Actions"));

        var assemblies = new List<Assembly>
        {
            actionsAssembly
        };

        foreach (var path in assemblyPaths)
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                assemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex,
                    string.Format("Error loading assembly from {0}{1}{2}", path, Environment.NewLine, ex.Message));
            }
        }

        return assemblies;
    }
    
    private static string GetActionName(string name)
    {
        return string.Concat("Get", name, "Action");
    }
    
    public static List<MethodInfo> GetFuncMethods(Type t) => t.GetMethods().Where(m => m.ReturnType.ToString().Contains("System.Func")).ToList();

    public static bool TryCreateInstance(string actionName, List<Type> types, out object obj)
    {
        obj = null;
        var type = types.FirstOrDefault(t => t.Name == actionName);
        if (type == null)
        {
            return false;
        }
        obj = Activator.CreateInstance(type);
        return true;
    }
    
    private static List<string> GetJsonParameters(string value, MethodInfo method)
    {
        List<string> result = new List<string>();

        Dictionary<string, string> jsonParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

        var methodParams = method.GetParameters();
        foreach (var p in methodParams)
        {
            if (jsonParameters.ContainsKey(p.Name))
            {
                result.Add(jsonParameters[p.Name]);
            }
            else if(p.IsOptional)
            {
                result.Add(p.HasDefaultValue && p.DefaultValue != null ? p.DefaultValue.ToString() : null);
            }
            else
            {
                LogHelper.LogDebug(string.Format("Parameter {0} is not available for action {1}", p.Name, method.Name));
                return null;
            }
        }
        return result;
    }
    /// <summary>
        /// Gets the parameters for the action. The parameters should match the action signature in the provided rules file
        /// </summary>
        /// <param name="value">The paramter(s) as a string or JSON object</param>
        /// <param name="method">The method for these parameters</param>
        /// <returns></returns>
        public static string[] GetParameters(dynamic value, MethodInfo method)
        {
            List<string> result = new List<string>();

            try
            {
                if (value is string)
                {
                    var strValue = value.ToString();
                    if (strValue.StartsWith("{"))
                    {
                        try
                        {
                            result = ActionLoaderUtils.GetJsonParameters(value.ToString(), method);
                        }
                        catch (Exception)
                        {
                            result = new List<string>() { value };
                        }
                    }
                    else
                    {
                        result = new List<string>() { value };
                        var optionalParameters = method.GetParameters().Where(p => p.IsOptional);
                        // This should only run if optional parameter was not inlcuded originally.
                        // TODO: We do not support ONLY optional parameters > 1 at this time, this logic would need to be re-written properly, that scenario would fail at val = (T)method.Invoke(invokeObject, parameters);
                        if (optionalParameters.Any() && method.GetParameters().Count() > 1) 
                        {
                            result.AddRange(optionalParameters.Select(p => p.HasDefaultValue && p.DefaultValue != null ? p.DefaultValue.ToString() : null));
                        }
                    }
                }
                else
                {
                    result = ActionLoaderUtils.GetJsonParameters(value.ToString(), method);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, "Error while loading parameters for action {0}", method.Name);
            }
            return result.ToArray();
        }
}
