namespace FullPotential.Api.Registry.Gear
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
