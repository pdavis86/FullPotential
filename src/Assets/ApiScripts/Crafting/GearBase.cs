// ReSharper disable UnusedMember.Global

namespace Assets.Scripts.Crafting.Results
{
    [System.Serializable]
    public abstract class GearBase : ItemBase
    {
        public string Type;
        public int Health;

        //todo: elemental resistance. e.g. strength 100 gives 25% damage reduction of element
        //todo: cosmetics
    }
}
