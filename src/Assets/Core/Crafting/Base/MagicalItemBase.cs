using Assets.Core.Crafting.SpellShapes;
using Assets.Core.Crafting.SpellTargeting;

// ReSharper disable ArrangeAccessorOwnerBody

namespace Assets.Core.Crafting.Base
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
