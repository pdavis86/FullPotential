namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessoryType : IRegisterableWithSlotType
    {
        int SlotCount { get; }
    }
}
