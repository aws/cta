using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NJsonSchema.Generation;
using NJsonSchema.Validation;

namespace CTA.Rules.Config
{
    public class Utils
    {
        public static int GenerateHashCode(int prime, string str)
        {
            int hash = 0;
            if (!string.IsNullOrEmpty(str))
            {
                hash = prime * str.GetHashCode();
            }
            return hash;
        }

        public static byte[] DownloadFromGitHub(string owner, string repo, string tag)
        {
            using var client = new HttpClient();
            //client.DefaultRequestHeaders.Add(HttpRequestHeader.Authorization.ToString(), string.Concat("token ", GithubInfo.TestGithubToken));
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3.raw"));
            client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("TestApp", "1.0.0.0"));

            var content = client.GetByteArrayAsync(string.Concat("https://api.github.com/repos/", owner, "/", repo, "/zipball/", tag)).Result;
            return content;
        }

        public static void SaveFileFromGitHub(string destination, string owner, string repo, string tag)
        {
            var content = Utils.DownloadFromGitHub(owner, repo, tag);
            File.WriteAllBytes(destination, content);
        }

        public static string GetRelativePath(string projectPath, string referencePath)
        {
            List<string> newPath = new List<string>();

            string[] projectPathArray = projectPath.Split(Path.DirectorySeparatorChar);
            string[] referencePathArray = referencePath.Split(Path.DirectorySeparatorChar);

            //Gets the index of the parent common folder
            int commonFolderIndex = 0;
            while (projectPathArray[commonFolderIndex] == referencePathArray[commonFolderIndex])
            {
                commonFolderIndex++;
            }

            var projectDepth = projectPathArray.Length - commonFolderIndex - 1;

            for (int i = 0; i < projectDepth; i++)
            {
                newPath.Add("..");
            }

            for (int i = commonFolderIndex; i < referencePathArray.Length; i++)
            {
                newPath.Add(referencePathArray[i]);
            }

            string relativePath = string.Join(Path.DirectorySeparatorChar.ToString(), newPath);
            return relativePath;
        }

        /// <summary>
        /// Use NJsonSchema to generate a schema from a class type and its properties, then validate
        /// that a json object conforms to the schema. Any validation errors will be reported via
        /// a thrown exception.
        /// </summary>
        /// <param name="jsonContent">Json content as a string</param>
        /// <param name="typeToValidate">Object type used to generate a validation schema</param>
        public static void ValidateJsonObject(string jsonContent, Type typeToValidate)
        {
            var schemaGeneratorSettings = new JsonSchemaGeneratorSettings();
            var schemaGenerator = new JsonSchemaGenerator(schemaGeneratorSettings);
            var schema = schemaGenerator.Generate(typeToValidate);

            var validator = new JsonSchemaValidator();
            var validationErrors = validator.Validate(jsonContent, schema);

            if (validationErrors.Any())
            {
                var errorMessageBuilder = new StringBuilder();
                var jsonContentLines = jsonContent.Split(Environment.NewLine);

                foreach (var error in validationErrors)
                {
                    var lineInfo = error.HasLineInfo ? $"Line {error.LineNumber}: {jsonContentLines[error.LineNumber].Trim()}" : string.Empty;
                    errorMessageBuilder.AppendLine($"{error} {lineInfo}");
                }

                throw new Newtonsoft.Json.JsonException($"Invalid {typeToValidate}:{Environment.NewLine}{errorMessageBuilder}");
            }
        }
        public static string EscapeAllWhitespace(string src) => Regex.Replace(src, @"(\s+)|(\\n)|(\\r)|(\\t)|(\n)|(\r)|(\t)", string.Empty);

        public static string DownloadFile(string fileUrl, string destinationFile, int retryCount = Constants.DownloadRetryCount)
        {
            int retryAttempts = 0;

            using (var httpClient = new HttpClient())
            {
                while (retryAttempts < retryCount)
                {
                    try
                    {
                        var fileContents = httpClient.GetByteArrayAsync(fileUrl).Result;
                        File.WriteAllBytes(destinationFile, fileContents);
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryAttempts++;
                        Thread.Sleep(Constants.DefaultThreadSleepTime);

                        if (retryAttempts == retryCount)
                        {
                            throw ex;
                        }
                    }
                }
            }

            return destinationFile;
        }

        /// <summary>
        /// Generates a unique file name by appending the number of Ticks to the original file name.
        /// A mutex is used so only 1 unique file name can be generated at a time, thus acting as
        /// enough of a delay to ensure each filename is unique.
        /// 
        /// Note: 1 tick is 100 ns
        /// </summary>
        /// <param name="fileName">Original name of file</param>
        /// <param name="extension">Extension to be appended to file name</param>
        /// <param name="mutexName">Identifier name of mutex</param>
        /// <param name="timeoutInSeconds">Time to wait for the mutex handle</param>
        /// <returns>File name with unique tick identifier and file extension appended to it</returns>
        public static string GenerateUniqueFileName(string fileName, string extension, string mutexName, int timeoutInSeconds = 5)
        {
            long now;
            using Mutex mutex = new Mutex(false, mutexName);
            if (mutex.WaitOne(timeoutInSeconds))
            {
                now = DateTime.Now.Ticks;
                mutex.ReleaseMutex();
            }
            else
            {
                // Mutex is used as a delay so if mutex wait time has been exceeded,
                // we can use current datetime anyway
                now = DateTime.Now.Ticks;
            }

            return $"{fileName}_{now}.{extension}";
        }
    }
}
