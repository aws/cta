using System;

namespace CTA.WebForms2Blazor.DirectiveConverters
{
    public class DirectiveMigrationResult : IEquatable<DirectiveMigrationResult>
    {
        public DirectiveMigrationResultType MigrationResultType { get; }
        public string Content { get; }

        public DirectiveMigrationResult(DirectiveMigrationResultType resultType, string content)
        {
            MigrationResultType = resultType;
            Content = content;
        }

        public bool Equals(DirectiveMigrationResult other)
        {
            // We can just use content because result type is
            // just going to be used for sorting purposes and
            // objects with the same content will always have the
            // same type
            return Content.Equals(other);
        }

        public override int GetHashCode()
        {
            // We can just use content because result type is
            // just going to be used for sorting purposes and
            // objects with the same content will always have the
            // same type
            return Content.GetHashCode();
        }
    }
}
