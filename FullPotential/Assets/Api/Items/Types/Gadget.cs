using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;

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
