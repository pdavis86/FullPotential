using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Items.Types
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
