using FullPotential.Api.Items.Base;

// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Gameplay.Items
{
    public interface IValueCalculator
    {
        int AddVariationToValue(double basicValue);
        int GetDamageValueFromAttack(ItemBase itemUsed, int targetDefense, bool addVariation = true);
    }
}