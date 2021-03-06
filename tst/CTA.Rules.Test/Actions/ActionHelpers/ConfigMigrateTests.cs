﻿using CTA.Rules.Actions;
using CTA.Rules.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;

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

            var loadWebConfigMethod = TestUtils.GetPrivateMethod(configMigrateType, "LoadWebConfig");
            var processWebConfigMethod = TestUtils.GetPrivateMethod(configMigrateType, "ProcessWebConfig");
            var addAppSettingsJsonFileMethod = TestUtils.GetPrivateMethod(configMigrateType, "AddAppSettingsJsonFile");

            var appSetting1 = @"ClientValidationEnabled";
            var appSetting2 = @"UnobtrusiveJavaScriptEnabled";
            var connectionStringWithBackSlash = @"Data Source=(LocalDb)\v11.0;Initial Catalog=ContosoUniversity2;Integrated Security=SSPI;";
            var connectionString2 = @"Data Source=Test;Initial Catalog=ContosoUniversity1;Integrated Security=SSPI;";

            var webConfig = string.Format(@"
<configuration>
    <appSettings>
        <add key=""webpages:Version"" value=""1.0.0.0""/>
        <add key=""{0}"" value=""1.0.0.0""/>
        <add key=""{1}"" value=""VALUEWITH\Backslash""/>
    </appSettings>
    <runtime>
        <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
            <dependentAssembly>
                <assemblyIdentity name=""System.Web.Mvc"" publicKeyToken=""31bf3856ad364e35""/>
                <bindingRedirect oldVersion=""3.0.0.0-3.0.0.1"" newVersion=""3.0.0.1""/>
            </dependentAssembly>
        </assemblyBinding>
    </runtime>
    <connectionStrings>
        <add name=""MusicStoreEntities"" connectionString=""{2}"" providerName=""System.Data.SqlClient""/>
        <add name=""MvcMusicStoreAuth"" connectionString=""{3}"" providerName=""System.Data.SqlClient"" />
    </connectionStrings> 
</configuration>
", appSetting1, appSetting2, connectionStringWithBackSlash, connectionString2);

            File.WriteAllText("web.config", webConfig);

            var templateContent = @"{}";
            var outputDir = "";

            var configuration = (Configuration)loadWebConfigMethod.Invoke(configMigrateInstance, new object[] { outputDir });

            // Invoke method and read contents of method output
            var content = (JObject)processWebConfigMethod.Invoke(configMigrateInstance, new object[] { configuration, templateContent });

            var methodParams = new object[] { content, outputDir };
            addAppSettingsJsonFileMethod.Invoke(configMigrateInstance, methodParams);

            var appSettingsContent = File.ReadAllText(Path.Combine(outputDir, "appsettings.json"));
            File.Delete(Path.Combine(outputDir, "appsettings.json"));
            File.Delete(Path.Combine(outputDir, "web.config"));

            Assert.True(appSettingsContent.Contains(appSetting1));
            Assert.True(appSettingsContent.Contains(appSetting2));
            Assert.True(appSettingsContent.Contains(connectionStringWithBackSlash.Replace(@"\", @"\\")));
            Assert.True(appSettingsContent.Contains(connectionString2));
        }

        [Test]
        public void AddConfigWithoutAppSettings()
        {
            // Get private method to invoke
            var projectType = ProjectType.ClassLibrary;
            ConfigMigrate configMigrateInstance = new ConfigMigrate(Directory.GetCurrentDirectory(), projectType);
            Type configMigrateType = configMigrateInstance.GetType();

            var loadWebConfigMethod = TestUtils.GetPrivateMethod(configMigrateType, "LoadWebConfig");
            var processWebConfigMethod = TestUtils.GetPrivateMethod(configMigrateType, "ProcessWebConfig");
            var addAppSettingsJsonFileMethod = TestUtils.GetPrivateMethod(configMigrateType, "AddAppSettingsJsonFile");

            var connectionStringWithBackSlash = @"Data Source=(LocalDb)\v11.0;Initial Catalog=ContosoUniversity2;Integrated Security=SSPI;";
            var connectionString2 = @"Data Source=Test;Initial Catalog=ContosoUniversity1;Integrated Security=SSPI;";

            var webConfig = string.Format(@"
<configuration>
    <runtime>
        <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
            <dependentAssembly>
                <assemblyIdentity name=""System.Web.Mvc"" publicKeyToken=""31bf3856ad364e35""/>
                <bindingRedirect oldVersion=""3.0.0.0-3.0.0.1"" newVersion=""3.0.0.1""/>
            </dependentAssembly>
        </assemblyBinding>
    </runtime>
    <connectionStrings>
        <add name=""MusicStoreEntities"" connectionString=""{0}"" providerName=""System.Data.SqlClient""/>
        <add name=""MvcMusicStoreAuth"" connectionString=""{1}"" providerName=""System.Data.SqlClient"" />
    </connectionStrings> 
</configuration>
", connectionStringWithBackSlash, connectionString2);

            File.WriteAllText("web.config", webConfig);

            var templateContent = @"{}";
            var outputDir = "";

            var configuration = (Configuration)loadWebConfigMethod.Invoke(configMigrateInstance, new object[] { outputDir });

            // Invoke method and read contents of method output
            var content = (JObject)processWebConfigMethod.Invoke(configMigrateInstance, new object[] { configuration, templateContent });

            var methodParams = new object[] { content, outputDir };
            addAppSettingsJsonFileMethod.Invoke(configMigrateInstance, methodParams);

            var appSettingsContent = File.ReadAllText(Path.Combine(outputDir, "appsettings.json"));
            File.Delete(Path.Combine(outputDir, "appsettings.json"));
            File.Delete(Path.Combine(outputDir, "web.config"));

            Assert.True(appSettingsContent.Contains(connectionStringWithBackSlash.Replace(@"\", @"\\")));
            Assert.True(appSettingsContent.Contains(connectionString2));
        }

        [Test]
        public void LoadWebConfigWithErrors()
        {
            // Get private method to invoke
            var projectType = ProjectType.ClassLibrary;
            ConfigMigrate configMigrateInstance = new ConfigMigrate(Directory.GetCurrentDirectory(), projectType);
            Type configMigrateType = configMigrateInstance.GetType();

            var loadWebConfigMethod = TestUtils.GetPrivateMethod(configMigrateType, "LoadWebConfig");


            var invalidWebConfig = @"
<configuration>
    <appSettings>
        <add key=""webpages:Version"" value=""1.0.0.0""/>
    </appSettings>
    <runtime>
        <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
            <dependentAssembly>
                <assemblyIdentity name=""System.Web.Mvc"" publicKeyToken=""31bf3856ad364e35""/>
                <bindingRedirect oldVersion=""3.0.0.0-3.0.0.1"" newVersion=""3.0.0.1""/>
            </dependentAssembly>
        </assemblyBinding>
    </runtime>
    <connectionStrings>
    </connectionStrings> 
    <UnclosedTag
</configuration>
";

            var projectDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            File.WriteAllText(Path.Combine(projectDir, "web.config"), invalidWebConfig);


            // Invoke method and read contents of method output
            var doc = (string)loadWebConfigMethod.Invoke(configMigrateInstance, new object[] { projectDir });

            Assert.Null(doc);
        }
    }
}