using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;

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
