using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Items
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
