using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CTA.Rules.Common.Extensions;

namespace CTA.Rules.Common.CsprojManagement
{
    public class CsprojXDocument
    {
        private readonly XDocument _csproj;

        private string _targetFrameworksValue;
        private string TargetFrameworksValue
        {
            get
            {
                if (string.IsNullOrEmpty(_targetFrameworksValue))
                {
                    _targetFrameworksValue = GetTargetFrameworksValue();
                }

                return _targetFrameworksValue;
            }
        }

        public CsprojXDocument(XDocument csproj)
        {
            _csproj = csproj ?? new XDocument();
        }

        public bool IsDotnetFramework()
        {
            return IsRegexMatch(Constants.DotnetFrameworkPattern, TargetFrameworksValue)
                   || IsRegexMatch(Constants.DotnetFrameworkSdkPattern, TargetFrameworksValue);
        }

        public bool IsDotnetCore()
        {
            return IsRegexMatch(Constants.DotnetCoreAppPattern, TargetFrameworksValue)
                   || IsRegexMatch(Constants.DotnetCorePattern, TargetFrameworksValue);
        }

        public bool IsDotnetStandard()
        {
            return IsRegexMatch(Constants.DotnetStandardPattern, TargetFrameworksValue);
        }

        private string GetTargetFrameworksValue()
        {
            var targetFrameworks =
                _csproj.GetElementsByLocalName(Constants.TargetFrameworkVersionElement).FirstOrDefault()
                ?? _csproj.GetElementsByLocalName(Constants.TargetFrameworkElement).FirstOrDefault()
                ?? _csproj.GetElementsByLocalName(Constants.TargetFrameworksElement).FirstOrDefault();

            return targetFrameworks?.Value ?? string.Empty;
        }

        private static bool IsRegexMatch(string regexPattern, string textToMatch)
        {
            var regex = new Regex(regexPattern);
            return regex.Match(textToMatch).Success;
        }
    }
}
