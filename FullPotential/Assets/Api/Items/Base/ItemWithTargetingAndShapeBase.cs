using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Api.Items.Base
{
    public abstract class ItemWithTargetingAndShapeBase : ItemBase, IHasTargetingAndShape
    {
        private ITargeting _targeting;
        private ITargetingVisuals _targetingVisuals;
        private IShape _shape;
        private IShapeVisuals _shapeVisuals;

        //Variables so they are serialized
        public string TargetingTypeId;
        public string TargetingVisualsTypeId;
        public string ShapeTypeId;
        public string ShapeVisualsTypeId;

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
            }
        }


        public ITargetingVisuals TargetingVisuals
        {
            get
            {
                //todo: if TargetingVisuals set against the item then use it
                //otherwise fallback to the default
                return null;
            }
            set
            {
                //todo: set TargetingVisuals
                //otherwise fallback to the default
            }
        }

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
            }
        }

        public IShapeVisuals ShapeVisuals 
        {
            get
            {
                //todo: if ShapeVisuals set against the item then use it
                //otherwise fallback to the default
                return null;
            }
            set
            {
                //todo: set ShapeVisuals
                //otherwise fallback to the default
            }
        }
    }
}
