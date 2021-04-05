namespace Assets.ApiScripts.Crafting
{
    public interface ICraftableArmor : ICraftable
    {
        public enum ArmorCategory
        {
            Helm,
            Chest,
            Legs,
            Feet,
            Barrier
        }

        ArmorCategory SubCategory { get; }
    }
}
