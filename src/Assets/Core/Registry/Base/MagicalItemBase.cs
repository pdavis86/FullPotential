using Assets.Core.Spells.Shapes;
using Assets.Core.Spells.Targeting;

// ReSharper disable ArrangeAccessorOwnerBody

namespace Assets.Core.Registry.Base
{
    [System.Serializable]
    public class MagicalItemBase : ItemBase, IMagical
    {
        public string TargetingTypeName;
        public string ShapeTypeName;

        private ISpellTargeting _targeting;
        public ISpellTargeting Targeting
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

        private ISpellShape _shape;
        public ISpellShape Shape
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
