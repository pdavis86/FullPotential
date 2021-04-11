using Assets.Core.Crafting.Base;

namespace Assets.Core.Crafting.Types
{
    [System.Serializable]
    public class Spell : ItemBase, IMagical
    {
        public string Targeting;
        public string Shape;

        public string GetTargetingTypeName()
        {
            return Targeting;
        }

        public string GetShapeTypeName()
        {
            return Shape;
        }
    }
}
