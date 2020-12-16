using System;
using System.IO;
using CTA.Rules.Actions;
using CTA.Rules.Models;
using NUnit.Framework;

namespace CTA.Rules.Test.Actions.ActionHelpers
{
    public class ConfigMigrateTests
    {
        [Test]
        public void AddAppSettingsJsonFile_Formats_Connection_Strings_With_A_Backslash()
        {
            // Get private method to invoke
            var projectType = ProjectType.ClassLibrary;
            ConfigMigrate configMigrateInstance = new ConfigMigrate(Directory.GetCurrentDirectory(), projectType);
            Type configMigrateType = configMigrateInstance.GetType();
            var methodName = "AddAppSettingsJsonFile";
            var addAppSettingsJsonFileMethod = TestUtils.GetPrivateMethod(configMigrateType, methodName);
            
            // Set parameters for method to be invoke
            var connectionStringValue =
                @"Data Source=(LocalDb)\v11.0;Initial Catalog=ContosoUniversity2;Integrated Security=SSPI;";
            var config = new configuration
            {
                connectionStrings = new [] 
                { 
                    new configurationAdd1
                    {
                        name = "ConnectionName",
                        connectionString = connectionStringValue
                    }
                }
            };
            var template = @"{}";
            var outputDir = "";
            var methodParams = new object[] {config, template, outputDir };

            // Invoke method and read contents of method output
            addAppSettingsJsonFileMethod.Invoke(configMigrateInstance, methodParams);
            var appSettingsContent = File.ReadAllText(Path.Combine(outputDir, "appsettings.json"));
            File.Delete(Path.Combine(outputDir, "appsettings.json"));

            Assert.True(appSettingsContent.Contains(connectionStringValue));
        }
    }
}