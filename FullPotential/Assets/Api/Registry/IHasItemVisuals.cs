using FullPotential.Api.Gameplay;

namespace FullPotential.Api.Registry
{
    public interface IHasItemVisuals
    {
        IItemVisuals Visuals { get; set; }

        void InstantiateItemVisuals(IPlayerFighter playerFighter, string slotId);
    }
}
