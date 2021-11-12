using CTA.Rules.Actions;
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
        public void AddConfigWithoutAppSettingsAndWithEncryptedConnectionString()
        {
            // Get private method to invoke
            var projectType = ProjectType.ClassLibrary;
            ConfigMigrate configMigrateInstance = new ConfigMigrate(Directory.GetCurrentDirectory(), projectType);
            Type configMigrateType = configMigrateInstance.GetType();

            var loadWebConfigMethod = TestUtils.GetPrivateMethod(configMigrateType, "LoadWebConfig");
            var processWebConfigMethod = TestUtils.GetPrivateMethod(configMigrateType, "ProcessWebConfig");
            var addAppSettingsJsonFileMethod = TestUtils.GetPrivateMethod(configMigrateType, "AddAppSettingsJsonFile");

            var encryptedConnectionString = (@" <EncryptedData Type=""http://www.w3.org/2001/04/xmlenc#Element""
      xmlns=""http://www.w3.org/2001/04/xmlenc#"">
      <EncryptionMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#aes256-cbc"" />
      <KeyInfo xmlns=""http://www.w3.org/2000/09/xmldsig#"">
        <EncryptedKey xmlns=""http://www.w3.org/2001/04/xmlenc#"">
          <EncryptionMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p"" />
          <KeyInfo xmlns=""http://www.w3.org/2000/09/xmldsig#"">
            <KeyName>Rsa Key</KeyName>
          </KeyInfo>
          <CipherData>
            <CipherValue>nkxQIoF9mSLILi28QYHJJgWdKARI7wi6Eobci45cobGuGqT6O7CDSTnDm2MEP1AGOkp/WYsWPZu+m1AVCwM+L9XNClKn0JQD/0EjW8Xt5J6EbNadQ0+jaWoeLdG5zuDDEzrKd3L13nAOuZngXCM0iKh1Lp9jVDojbcaXVt88cK5SD6V0z/8ug+7g+sZqLVJd6zyRzBenbNB5w+XVUCcLkAIfgBlvLvx26CrZ2sq36iRmmO1xM1VkXvKfMDb0Mu4UTwNcxB17fipgj4eu5AUfQj+nKbagSwFVmOOw92rVYr6jgveElgoSiqzL/sN01eT+xCKDWXqcS7T5ks5QQIIh/A==</CipherValue>
          </CipherData>
        </EncryptedKey>
      </KeyInfo>
      <CipherData>
        <CipherValue>TD/NPQ/BWh1C95odOotarRZDT3pENWUEKbGULkBFE/iL39rq7L5HvxgezKqz6YKLhUm2LyU05VE03dGPP5yJQVW6bAJjHIC47hVzlzIRehx7ihk4yDqgrROpwmGl9zw1n/V+QDwrqnkYOPZE9ubZsgPPSaWf7/FwtrbpRbWLXLzmBT4LRxOBeZLmSM40XMYkZgQiUAWNw6tu6XiFg7y/kbBXGa2jzoAXPaxcMqjhyQfVGyDhirOh5vmSJJV+kkiZ43KQIv/eoKv6pylHnocP0rW05y5Jl1YfgsiXJVqhDFYsd8wHqUe5iuOwqE4n5KiDwf37Z6HRwnnCKsw2O6bzud4lEsKjFte/FpL/esBxrQvCAmDIgix8UEadDqlCG3cG</CipherValue>
      </CipherData>
    </EncryptedData>");
            var connectionString2 = @"Data Source=Test;Initial Catalog=ContosoUniversity1;Integrated Security=SSPI;";
            var defaultConnectionString = @"data source=.\\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|aspnetdb.mdf;User Instance=true";
            var connectionStrings = string.Format(@"<connectionStrings configProtectionProvider=""RsaProtectedConfigurationProvider"">
                                                        {0}
                                                        <add name=""MvcMusicStoreAuth"" connectionString=""{1}"" providerName=""System.Data.SqlClient"" />
                                                        </connectionStrings>",
                                                        encryptedConnectionString, connectionString2);

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
                                          {0}
                                 </configuration>",
                                 connectionStrings);

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

            Assert.True(appSettingsContent.Contains(defaultConnectionString));
        }

        /// <summary>
        /// The LoadWebConfigWithErrors.
        /// </summary>
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
