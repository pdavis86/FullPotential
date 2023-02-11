using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Items.Types
{
    [System.Serializable]
    public class Spell : SpellOrGadgetItemBase
    {
        public Spell()
        {
            ResourceConsumptionType = ResourceConsumptionType.Mana;
        }
    }
}
