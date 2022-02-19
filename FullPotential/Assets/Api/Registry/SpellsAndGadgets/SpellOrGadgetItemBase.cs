using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry.Base;

namespace FullPotential.Api.Registry.SpellsAndGadgets
{
    [System.Serializable]
    public abstract class SpellOrGadgetItemBase : ItemBase, ISpellOrGadget
    {
        public string TargetingTypeName;
        public string ShapeTypeName;

        public ResourceConsumptionType ResourceConsumptionType { get; protected set; }

        private ITargeting _targeting;
        public ITargeting Targeting
        {
            get
            {
                return _targeting;
            }
            set
            {
                _targeting = value;
                TargetingTypeName = _targeting?.TypeName;
            }
        }

        private IShape _shape;
        public IShape Shape
        {
            get
            {
                return _shape;
            }
            set
            {
                _shape = value;
                ShapeTypeName = _shape?.TypeName;
            }
        }

    }
}
