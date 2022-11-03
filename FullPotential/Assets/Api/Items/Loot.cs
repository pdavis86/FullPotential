using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Api.Items
{
    [System.Serializable]
    public class Loot : ItemBase, IHasTargetingAndShape
    {
        public ITargeting Targeting { get; set; }

        public string TargetingTypeName { get; set; }

        public IShape Shape { get; set; }

        public string ShapeTypeName { get; set; }
    }
}
