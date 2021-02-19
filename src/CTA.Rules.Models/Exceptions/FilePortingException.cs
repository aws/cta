using System;

namespace CTA.Rules.Models
{
    public class FilePortingException : Exception
    {
        public FilePortingException(string filename, Exception exception)
            : base(GetActionExecutionException(filename), exception)
        {

        }

        public static string GetActionExecutionException(string filename)
        {
            return string.Format("Error while running actions on file {0}", filename);
        }
    }
}
