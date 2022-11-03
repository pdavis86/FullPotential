namespace FullPotential.Api.Registry.Crafting
{
    public interface IGear : ICraftable
    {
        public enum GearCategory
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
