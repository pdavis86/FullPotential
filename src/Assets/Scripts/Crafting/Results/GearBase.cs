// ReSharper disable UnusedMember.Global

namespace Assets.Scripts.Crafting.Results
{
    public abstract class GearBase : ItemBase
    {
        public string Type { get; set; }
        public int Health { get; set; }

        //todo: elemental resistance. e.g. strength 100 gives 25% damage reduction of element
    }
}
