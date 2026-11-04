using FullPotential.Api.Gameplay;
using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Api.Registry.Visuals
{
    public interface IHasItemVisuals
    {
        IItemVisualsType Visuals { get; set; }

        void InstantiateItemVisuals(IPlayerFighter playerFighter, SlotId slotId);
    }
}
