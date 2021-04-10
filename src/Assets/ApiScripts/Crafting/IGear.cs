namespace Assets.ApiScripts.Crafting
{
    public interface IGear : ICraftable
    {
        public enum InventorySlots
        {
            Helm,
            Chest,
            Legs,
            Feet,
            Barrier,

            Hand,

            Ring,
            Belt,
            Amulet
        }

        //todo: need to register a prefab when registering
    }
}
