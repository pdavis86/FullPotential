using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay;

namespace FullPotential.Api.Registry
{
    public interface IHasItemVisuals
    {
        IItemVisualsType Visuals { get; set; }

        void InstantiateItemVisuals(IPlayerFighter playerFighter, SlotId slotId);
    }
}
