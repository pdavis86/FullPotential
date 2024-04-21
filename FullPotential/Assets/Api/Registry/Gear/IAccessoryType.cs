namespace FullPotential.Api.Registry.Gear
{
    public interface IAccessoryType : IRegisterableWithSlotType
    {
        public int SlotCount { get; }
    }
}
