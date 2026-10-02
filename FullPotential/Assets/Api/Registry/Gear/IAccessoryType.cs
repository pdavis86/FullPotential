namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessoryType : ISlotType
    {
        int SlotCount { get; }
    }
}
