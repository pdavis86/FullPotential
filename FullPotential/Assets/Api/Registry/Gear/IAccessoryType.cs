namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessoryType : ICraftableType, ISlotType
    {
        int SlotCount { get; }
    }
}
