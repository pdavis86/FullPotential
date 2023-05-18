using System;
using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Api.Registry.Targeting
{
    public interface ITargetingVisuals : IRegisterable, IHasPrefab
    {
        Guid TargetingTypeId { get; }

        bool IsParentedToSource { get; }
    }
}
