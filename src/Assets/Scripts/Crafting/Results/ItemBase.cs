// ReSharper disable UnusedMember.Global

namespace Assets.Scripts.Crafting.Results
{
    public abstract class ItemBase : CraftableBase
    {
        public string Type { get; set; }
        public int Health { get; set; }
    }
}
