namespace Assets.Scripts.Ui.Crafting.Items
{
    public abstract class ItemBase : CraftableBase
    {
        public string Type { get; set; }
        public int Health { get; set; }
    }
}
