namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessory : IRegisterableWithSlot, IHasEventHandlers
    {
        public int SlotCount { get; }
    }
}
