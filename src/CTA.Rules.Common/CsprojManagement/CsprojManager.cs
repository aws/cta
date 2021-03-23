using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CTA.Rules.Config;

namespace CTA.Rules.Common.CsprojManagement
{
    public class CsprojManager
    {
        private static Dictionary<string, XDocument> _xDocumentCache;
        private static Dictionary<string, XDocument> XDocumentCache
        {
            get
            {
                _xDocumentCache ??= new Dictionary<string, XDocument>();
                return _xDocumentCache;
            }

            set
            {
                _xDocumentCache = value;
            }
        } 

        private delegate object CsprojLoadingDelegate(string csprojFilePath);

        public static CsprojXDocument LoadCsprojAsXDocument(string projectDir)
        {
            var csproj = LoadCsproj(projectDir, csprojFile =>
            {
                if (XDocumentCache.TryGetValue(csprojFile, out var cached))
                {
                    return cached;
                }

                var xDocument = XDocument.Load(csprojFile);
                XDocumentCache[csprojFile] = xDocument;

                return XDocumentCache[csprojFile];
            }) as XDocument;

            return new CsprojXDocument(csproj);
        }

        public static void ClearCache()
        {
            XDocumentCache = null;
        }

        private static object LoadCsproj(string csprojFilePath, CsprojLoadingDelegate csprojLoadingDelegate)
        {
            if (File.Exists(csprojFilePath))
            {
                try
                {
                    return csprojLoadingDelegate.Invoke(csprojFilePath);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(ex, string.Format("Error processing .csproj file {0}", csprojFilePath));
                }
            }
            return null;
        }
    }
}
