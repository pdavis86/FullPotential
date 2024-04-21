using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Api.Items
{
    public interface IResourceConsumer
    {
        public IResourceType ResourceType { get; }

        int GetResourceCost();
    }
}
