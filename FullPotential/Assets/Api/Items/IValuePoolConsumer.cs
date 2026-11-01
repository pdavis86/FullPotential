using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Api.Items
{
    public interface IValuePoolConsumer : IItem
    {
        // todo: zzz v0.7 - Remove backwards-compatible PropertyKeyResourceTypeId
        const string PropertyKeyResourceTypeId = "ResourceTypeId";

        const string PropertyKeyValuePoolTypeId = "ValuePoolTypeId";

        IValuePoolType ValuePoolType { get; set; }

        int GetValuePoolCost();
    }
}
