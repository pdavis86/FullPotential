using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Api.Items
{
    public interface IHasTargetingAndShape
    {
        ITargeting Targeting { get; set; }

        IShape Shape { get; set; }
    }
}
