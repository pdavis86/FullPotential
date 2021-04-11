using Assets.Core.Crafting.Base;
using Assets.Core.Crafting.Types;

namespace Assets.Core.Crafting
{
    [System.Serializable]
    public class Loot : CraftableBase, IMagical
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
