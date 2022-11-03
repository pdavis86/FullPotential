using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Api.Items
{
    public interface IHasTargetingAndShape
    {
        ITargeting Targeting { get; set; }

        public string TargetingTypeName { get; }

        IShape Shape { get; set; }

        public string ShapeTypeName { get; }
    }
}
