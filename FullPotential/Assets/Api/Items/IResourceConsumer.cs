using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Api.Items
{
    public interface IResourceConsumer : IItem
    {
        public const string PropertyKeyResourceTypeId = "ResourceTypeId";

        IResourceType ResourceType { get; set; }

        int GetResourceCost();
    }
}
