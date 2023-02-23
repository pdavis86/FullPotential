using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Api.Registry.Targeting
{
    public interface ITargetingVisuals<TTargeting> : IRegisterable, IHasPrefab
        where TTargeting : ITargeting
    {
        bool IsParentedToSource { get; }
    }
}
