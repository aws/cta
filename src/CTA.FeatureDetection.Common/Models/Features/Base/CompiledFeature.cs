namespace CTA.FeatureDetection.Common.Models.Features.Base
{
    /// <summary>
    /// Base class for Features that require compilation
    /// </summary>
    public abstract class CompiledFeature : Feature
    {
        private string _name;
        public override string Name
        {
            get => _name ?? GetType().Name;
            set => _name = value;
        }
    }
}