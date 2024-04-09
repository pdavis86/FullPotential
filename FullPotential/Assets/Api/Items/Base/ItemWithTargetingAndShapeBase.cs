using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Api.Items.Base
{
    //todo: zzz v0.5 - Remove all the Serializable nonsense and make this an interface instead of an abstract class
    public abstract class ItemWithTargetingAndShapeBase : CombatItemBase, IHasTargetingAndShape
    {
        private ITargeting _targeting;
        private ITargetingVisuals _targetingVisuals;
        private IShape _shape;
        private IShapeVisuals _shapeVisuals;

        //Variables so they are serialized
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable NotAccessedField.Global
        public string TargetingTypeId;
        public string TargetingVisualsTypeId;
        public string ShapeTypeId;
        public string ShapeVisualsTypeId;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore NotAccessedField.Global

        public ITargeting Targeting
        {
            get => _targeting;
            set
            {
                _targeting = value;
                TargetingTypeId = _targeting?.TypeId.ToString();
            }
        }


        public ITargetingVisuals TargetingVisuals
        {
            get => _targetingVisuals;
            set
            {
                _targetingVisuals = value;
                TargetingVisualsTypeId = _targetingVisuals?.TypeId.ToString();
            }
        }

        public IShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                ShapeTypeId = _shape?.TypeId.ToString();
            }
        }

        public IShapeVisuals ShapeVisuals
        {
            get => _shapeVisuals;
            set
            {
                _shapeVisuals = value;
                ShapeVisualsTypeId = _shapeVisuals?.TypeId.ToString();
            }
        }
    }
}
