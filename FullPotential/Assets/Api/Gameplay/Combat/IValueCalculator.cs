using FullPotential.Api.Registry;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IValueCalculator
    {
        int GetAttackValue(Attributes? attributes, int targetDefense);
        int GetVelocityDamage(Vector3 velocity);
        int AddVariationToValue(double basicValue);
    }
}