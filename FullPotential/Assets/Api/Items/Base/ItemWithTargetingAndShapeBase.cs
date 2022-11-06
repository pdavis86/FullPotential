using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Api.Items.Base
{
    public abstract class ItemWithTargetingAndShapeBase : ItemBase, IHasTargetingAndShape
    {
        //todo: obsolete but left in until data is fixed
        public string TargetingTypeName;
        public string ShapeTypeName;

        //Variables so they are serialized
        public string TargetingTypeId;
        public string ShapeTypeId;

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
                TargetingTypeId = _targeting?.TypeId.ToString();
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
                ShapeTypeId = _shape?.TypeId.ToString();
                ShapeTypeName = _shape?.TypeName;
            }
        }
    }
}
