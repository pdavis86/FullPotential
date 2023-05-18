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
        // ReSharper disable MemberCanBePrivate.Global
        public string TargetingTypeId;
        public string TargetingVisualsTypeId;
        public string ShapeTypeId;
        public string ShapeVisualsTypeId;
        // ReSharper restore MemberCanBePrivate.Global

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
                return _targetingVisuals;
            }
            set
            {
                _targetingVisuals = value;
                TargetingVisualsTypeId = _targetingVisuals?.TypeId.ToString();
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
                return _shapeVisuals;
            }
            set
            {
                _shapeVisuals = value;
                ShapeVisualsTypeId = _shapeVisuals?.TypeId.ToString();
            }
        }
    }
}
