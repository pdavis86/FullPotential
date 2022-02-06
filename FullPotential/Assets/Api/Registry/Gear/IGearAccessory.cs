namespace FullPotential.Api.Registry.Gear
{
    public interface IGearAccessory : IGear
    {
        public enum AccessoryCategory
        {
            Ring = GearCategory.Ring,
            Belt = GearCategory.Belt,
            Amulet = GearCategory.Amulet
        }

        AccessoryCategory Category { get; }
    }
}
