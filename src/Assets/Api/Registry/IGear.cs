namespace Assets.ApiScripts.Registry
{
    public interface IGear : ICraftable
    {
        public enum GearSlot
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

    }
}
