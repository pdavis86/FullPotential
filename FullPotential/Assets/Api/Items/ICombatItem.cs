using FullPotential.Api.Registry;

namespace FullPotential.Api.Items
{
    public interface ICombatItem : IItem, IHasEffects, IHasCharge
    {
        bool IsTwoHanded { get; set; }

        string ShapeCode { get; set; }

        int GetNameHash();

        float GetDamagePerSecond();

        float GetAmmoPerSecond();
    }
}
