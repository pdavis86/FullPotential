using FullPotential.Api.Gameplay.Enums;

namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    [System.Serializable]
    public class Gadget : SpellOrGadgetItemBase
    {
        public Gadget()
        {
            ResourceConsumptionType = ResourceConsumptionType.Energy;
        }

    }
}
