using FullPotential.Api.Gameplay;

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Api.Registry
{
    public interface IHasItemVisuals
    {
        IItemVisualsType Visuals { get; set; }

        void InstantiateItemVisuals(IPlayerFighter playerFighter, SlotId slotId);
    }
}
