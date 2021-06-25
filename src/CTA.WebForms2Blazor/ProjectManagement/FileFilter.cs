using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace CTA.WebForms2Blazor.ProjectManagement
{
    public static class FileFilter
    {
        // CTA .gitignore file was used as a model for
        // these conditions, most were extracted but not
        // all seemed relevant

        // Maybe loading these in from an external file
        // is better in the long run, can modify later
        // if we want to
        public static readonly ImmutableArray<string> ContainsAcceptConditions = ImmutableArray.Create(new string[]
        {
            string.Format(".cache{0}", Path.DirectorySeparatorChar),
            string.Format("{0}packages{0}build{0}", Path.DirectorySeparatorChar)
        });

        public static readonly ImmutableArray<string> ContainsIgnoreConditions = ImmutableArray.Create(new string[]
        {
            string.Format(".vshistory{0}", Path.DirectorySeparatorChar),
            string.Format("{0}.vs{0}", Path.DirectorySeparatorChar),
            string.Format("{0}Debug{0}", Path.DirectorySeparatorChar),
            string.Format("{0}DebugPublic{0}", Path.DirectorySeparatorChar),
            string.Format("{0}Release{0}", Path.DirectorySeparatorChar),
            string.Format("{0}Releases{0}", Path.DirectorySeparatorChar),
            string.Format("{0}x64{0}", Path.DirectorySeparatorChar),
            string.Format("{0}x86{0}", Path.DirectorySeparatorChar),
            string.Format("{0}build{0}", Path.DirectorySeparatorChar),
            string.Format("{0}bld{0}", Path.DirectorySeparatorChar),
            string.Format("{0}bin{0}", Path.DirectorySeparatorChar),
            string.Format("{0}obj{0}", Path.DirectorySeparatorChar),
            string.Format("{0}testResult{0}", Path.DirectorySeparatorChar),
            string.Format("{0}buildLog{0}", Path.DirectorySeparatorChar),
            string.Format("{0}debugPS{0}", Path.DirectorySeparatorChar),
            string.Format("{0}releasePS{0}", Path.DirectorySeparatorChar),
            string.Format("{0}artifacts{0}", Path.DirectorySeparatorChar),
            string.Format("{0}publish{0}", Path.DirectorySeparatorChar),
            string.Format("{0}packages{0}", Path.DirectorySeparatorChar)
        });

        public static readonly ImmutableArray<string> EndIgnoreConditions = ImmutableArray.Create(new string[]
        {
            ".userosscache", ".userprefs", ".suo", ".user", ".ilk", ".meta", ".obj", ".pch", ".pdb", ".pgc",
            ".pgd", ".rsp", ".sbr", ".tlb", "tli", ".tlh", ".tmp", ".tmp_proj", ".log", ".vspscc", "vssscc",
            ".builds", ".pidb", ".svclog", ".scc", ".nupkg", ".cache", ".sln.docstates", ".visualState.xml",
            "_i.c", "_p.c", "_i.h",
            string.Format("{0}dlldata.c", Path.DirectorySeparatorChar),
            string.Format("{0}launchSettings.json", Path.DirectorySeparatorChar),
            string.Format("{0}testResult.xml", Path.DirectorySeparatorChar),
            string.Format("{0}project.lock.json", Path.DirectorySeparatorChar),
            string.Format("{0}package.config", Path.DirectorySeparatorChar)
        });

        public static bool ShouldIgnoreFileAtPath(string relativePath)
        {
            var modifiedRelativePath = Path.DirectorySeparatorChar + relativePath;

            // Must check accept conditions first
            // NOTE: string.Contains more efficient than regex checks
            if (ContainsAcceptConditions.Any(condition => modifiedRelativePath.Contains(condition, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }

            if (ContainsIgnoreConditions.Any(condition => modifiedRelativePath.Contains(condition, StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            if (EndIgnoreConditions.Any(condition => modifiedRelativePath.EndsWith(condition, StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}
