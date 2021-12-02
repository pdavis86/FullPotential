using FullPotential.Api.Behaviours;
using FullPotential.Core.Registry.Base;

namespace FullPotential.Core.Registry.Types
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
