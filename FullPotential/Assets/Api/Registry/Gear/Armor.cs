using FullPotential.Api.Gameplay.Combat;

namespace FullPotential.Api.Registry.Gear
{
    [System.Serializable]
    public class Armor : GearBase, IDefensible
    {
        public int GetDefenseValue()
        {
            return Attributes.Strength;
        }
    }
}
