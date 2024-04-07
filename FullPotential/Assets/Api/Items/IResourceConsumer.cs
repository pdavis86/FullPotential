using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Api.Items
{
    public interface IResourceConsumer
    {
        public IResource ResourceType { get; }

        int GetResourceCost();
    }
}
