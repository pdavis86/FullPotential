namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessory : IRegisterableWithSlot
    {
        public int SlotCount { get; }
    }
}
