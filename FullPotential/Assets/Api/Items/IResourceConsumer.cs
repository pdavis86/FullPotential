using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Api.Items
{
    public interface IResourceConsumer
    {
        IResourceType ResourceType { get; }

        int GetResourceCost();
    }
}
