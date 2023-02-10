using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    public interface IShapeBehaviour
    {
        SpellOrGadgetItemBase SpellOrGadget { set; }

        IFighter SourceFighter { set; }
    }
}
