namespace FullPotential.Api.Registry
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
