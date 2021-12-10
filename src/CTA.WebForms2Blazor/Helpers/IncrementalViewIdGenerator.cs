namespace CTA.WebForms2Blazor.Helpers
{
    public static class IncrementalViewIdGenerator
    {
        public static int NextGeneratedIdNumber { get; private set; } = 0;

        private const string GeneratedIdentifierTemplate = "GeneratedId{0}";

        public static string GetNewGeneratedId()
        {
            var newGeneratedId = string.Format(GeneratedIdentifierTemplate, NextGeneratedIdNumber);
            NextGeneratedIdNumber += 1;

            return newGeneratedId;
        }
    }
}
