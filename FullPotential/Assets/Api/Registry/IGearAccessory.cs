namespace FullPotential.Assets.Api.Registry
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
