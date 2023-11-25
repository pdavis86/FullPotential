using FullPotential.Api.Gameplay.Combat;

namespace FullPotential.Api.Items
{
    public interface IResourceConsumer
    {
        public IResource ResourceType { get; }

        int GetResourceCost();
    }
}
