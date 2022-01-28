using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using NJsonSchema.Generation;
using NJsonSchema.Validation;

namespace CTA.Rules.Config
{
    public class Utils
    {
        private const string DefaultMutexName = "DefaultMutex";

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
                    catch (Exception)
                    {
                        retryAttempts++;
                        Thread.Sleep(Constants.DefaultThreadSleepTime);

                        if (retryAttempts == retryCount)
                        {
                            throw;
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
        /// <param name="filePath">Original name of file</param>
        /// <param name="mutexName">Identifier name of mutex</param>
        /// <param name="timeoutInSeconds">Time to wait for the mutex handle</param>
        /// <returns>File name with unique tick identifier and file extension appended to it</returns>
        public static string GenerateUniqueFileName(string filePath, string mutexName, int timeoutInSeconds = 5)
        {
            string now;
            using Mutex mutex = new Mutex(false, mutexName);
            if (mutex.WaitOne(timeoutInSeconds))
            {
                now = DateTime.Now.ToString("yyyyMMdd_HH_mm_ss_fffffff");
                mutex.ReleaseMutex();
            }
            else
            {
                // Mutex is used as a delay so if mutex wait time has been exceeded,
                // we can use current datetime anyway
                now = DateTime.Now.ToString("yyyyMMdd_HH_mm_ss_fffffff");
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            return $"{fileName}_CONFLICT_{now}{extension}";
        }

        /// <summary>
        /// Writes string content to a file in a thread-safe manner
        /// </summary>
        /// <param name="filePath">File to write string content</param>
        /// <param name="content">String content to persist</param>
        /// <param name="fileShare">FileShare mode</param>
        /// <param name="mutexName">Mutex identifier</param>
        /// <returns>File path that was written to</returns>
        /// <exception cref="IOException">Throws if there is an unexpected IOException during writing</exception>
        public static string ThreadSafeExportStringToFile(string filePath, string content, FileShare fileShare = FileShare.ReadWrite, string mutexName = DefaultMutexName)
        {
            try
            {
                using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, fileShare);
                using var streamWriter = new StreamWriter(fileStream);
                streamWriter.Write(content);

                return filePath;
            }
            catch (IOException)
            {
                // IOException is thrown if filePath is locked by an external process
                // If this happens, generate a unique identifier, append it to the file name,
                // and try writing again.
                var uniqueFileName = GenerateUniqueFileName(filePath, mutexName);

                using var fileStream = File.Open(uniqueFileName, FileMode.Create, FileAccess.ReadWrite, fileShare);
                using var streamWriter = new StreamWriter(fileStream);
                streamWriter.Write(content);

                return uniqueFileName;
            }
        }


        public static string CopySolutionToDestinationLocation(string solutionName, string solutionPath)
        {
            string slnDirPath = Directory.GetParent(solutionPath).FullName;
            string root = Path.GetPathRoot(slnDirPath);
            string relativeSrc = Path.GetRelativePath(root, slnDirPath);
            
            var newTempDirPath = Path.Combine(Directory.GetParent(slnDirPath).FullName, Guid.NewGuid().ToString());
            string destPath = Path.Combine(newTempDirPath, relativeSrc);
            string newSlnPath = CopyFolderToTemp(solutionName, slnDirPath, destPath);


            if (solutionPath.Contains(".sln") && File.Exists(solutionPath))
            {
                IEnumerable<string> projects = GetProjectPathsFromSolutionFile(solutionPath);
                foreach (string project in projects)
                {
                    string projPath = Directory.GetParent(project).FullName;
                    relativeSrc = Path.GetRelativePath(solutionPath, projPath);
                    string projName = Path.GetFileName(project);

                    if (!IsSubPathOf(slnDirPath, projPath))
                    {
                        string relDestPath = Path.Combine(destPath, relativeSrc);
                        CopyFolderToTemp(projName, projPath, relDestPath);
                    }
                }
            }
            return newSlnPath;
        }



        /// <summary>
        /// Copies a solution to a new location under a folder with a randomly generated name
        /// </summary>
        /// <param name="solutionName">The name of the solution (MySolution.sln)</param>
        /// <param name="tempDir">The folder the location resides in</param>
        /// <returns></returns>
        public static string CopyFolderToTemp(string solutionName, string tempDir, string destinationLocation)
        {
            string solutionPath = Directory.EnumerateFiles(tempDir, solutionName, SearchOption.AllDirectories).FirstOrDefault(s => !s.Contains(string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar)));
            string solutionDir = Directory.GetParent(solutionPath).FullName;
            CopyDirectory(new DirectoryInfo(solutionDir), new DirectoryInfo(destinationLocation));

            solutionPath = Directory.EnumerateFiles(destinationLocation, solutionName, SearchOption.AllDirectories).FirstOrDefault();
            return solutionPath;
        }

        /// <summary>
        /// Copies a directory to another folder
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="target">Destination directory</param>
        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            var files = source.GetFiles();
            foreach (var file in files)
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }

            var dirs = source.GetDirectories();
            foreach (var dir in dirs)
            {
                DirectoryInfo destinationSub = new DirectoryInfo(Path.Combine(target.FullName, dir.Name));
                CopyDirectory(dir, destinationSub);
            }
        }


        public static void DownloadFilesToFolder(string s3Bucket, string targetFolder, List<List<string>> files)
        {
            using var httpClient = new HttpClient();

            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Constants.ThreadCount };

            Parallel.ForEach(Constants.TemplateFiles, file =>
            {
                var localFile = Path.Combine(targetFolder, string.Join(Path.DirectorySeparatorChar, file));
                var remoteFile = string.Concat(s3Bucket, "/", string.Join("/", file));

                if (File.Exists(localFile))
                {
                    var lastModified = File.GetLastWriteTime(localFile);
                    //File doesn't need to be refreshed
                    if (lastModified.AddDays(Constants.CacheExpiryDays) > DateTime.Now)
                    {
                        return;
                    }
                }

                try
                {
                    var fileContent = httpClient.GetStringAsync(remoteFile).Result;
                    Directory.CreateDirectory(Path.GetDirectoryName(localFile));
                    File.WriteAllText(localFile, fileContent);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, $"Error while dowloading file {file}");
                }
            });
        }

        public static IEnumerable<string> GetProjectPathsFromSolutionFile(string solutionPath)
        {

            IEnumerable<string> projectPaths = null;
            try
            {
                SolutionFile solution = SolutionFile.Parse(solutionPath);
                projectPaths = solution.ProjectsInOrder.Where(p => AcceptedProjectTypes.Contains(p.ProjectType)).Select(p => p.AbsolutePath);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, $"Error while parsing the solution file {solutionPath}");
            }

            return projectPaths;

        }

        public static HashSet<SolutionProjectType> AcceptedProjectTypes = new HashSet<SolutionProjectType>()
        {
            SolutionProjectType.KnownToBeMSBuildFormat,
            SolutionProjectType.WebDeploymentProject,
            SolutionProjectType.WebProject
        };

        public static bool IsSubPathOf(string subPath, string basePath)
        {
            DirectoryInfo subDir = new DirectoryInfo(subPath);
            DirectoryInfo baseDir = new DirectoryInfo(basePath);
            bool isParent = false;
            while (baseDir.Parent != null)
            {
                if (baseDir.Parent.FullName == subDir.FullName)
                {
                    isParent = true;
                    break;
                }
                else
                {
                    baseDir = baseDir.Parent;
                }
            }
            return isParent;
        }
    }
}
