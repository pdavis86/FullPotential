namespace FullPotential.Assets.Api.Registry
{
    public interface IGearArmor : IGear
    {
        public enum ArmorCategory
        {
            Helm = GearCategory.Helm,
            Chest = GearCategory.Chest,
            Legs = GearCategory.Legs,
            Feet = GearCategory.Feet,
            Barrier = GearCategory.Barrier
        }

        ArmorCategory Category { get; }
    }
}
