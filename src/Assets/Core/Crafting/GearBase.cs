// ReSharper disable UnusedMember.Global

namespace Assets.Core.Crafting
{
    [System.Serializable]
    public abstract class GearBase : ItemBase
    {
        public string SubType;
        public int Health;

        //todo: elemental resistance. e.g. strength 100 gives 25% damage reduction of element
        //todo: cosmetics
    }
}
