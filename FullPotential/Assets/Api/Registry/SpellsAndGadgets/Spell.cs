using FullPotential.Api.Gameplay.Enums;

namespace FullPotential.Api.Registry.SpellsAndGadgets
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
