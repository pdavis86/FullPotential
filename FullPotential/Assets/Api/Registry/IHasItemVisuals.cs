using FullPotential.Api.Gameplay;

namespace FullPotential.Api.Registry
{
    public interface IHasItemVisuals
    {
        IItemVisuals Visuals { set; }

        void InstantiateItemVisuals(IPlayerFighter playerFighter, string slotId);
    }
}
