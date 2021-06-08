using System;

namespace CTA.WebForms2Blazor
{
    public class MigrationManager
    {
        private readonly string _sourceProjectPath;
        private readonly string _outputProjectPath;

        public MigrationManager(string sourceProjectPath, string outputProjectPath)
        {
            _sourceProjectPath = sourceProjectPath;
            _outputProjectPath = outputProjectPath;

            // Initialize any objects or services needed for migration here
        }

        public void PerformMigration()
        {
            // Central migration logic goes here
        }
    }
}
