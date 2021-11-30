using FullPotential.Assets.Api.Behaviours;
using FullPotential.Assets.Core.Registry.Base;

namespace FullPotential.Assets.Core.Registry.Types
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
