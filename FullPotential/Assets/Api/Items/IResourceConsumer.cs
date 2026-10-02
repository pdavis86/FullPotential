using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Api.Items
{
    public interface IResourceConsumer
    {
        string Name { get; }

        IResourceType ResourceType { get; }

        int GetResourceCost();
    }
}
