using FullPotential.Api.Registry.Consumers;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Api.Items.Base
{
    public abstract class ItemWithTargetingAndShapeBase : ItemBase, IHasTargetingAndShape
    {
        //Variables so they are serialized
        public string TargetingTypeId;
        public string TargetingVisualsTypeId;
        public string ShapeTypeId;
        public string ShapeVisualsTypeId;

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
            }
        }


        public ITargetingVisuals<ITargeting> TargetingVisuals
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
            }
        }

        public IShapeVisuals<IShape> ShapeVisuals 
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
