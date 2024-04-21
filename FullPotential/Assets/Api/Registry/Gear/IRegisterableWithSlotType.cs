namespace FullPotential.Api.Registry.Gear
{
    public interface IRegisterableWithSlotType : IRegisterableType
    {
        public string SlotSpritePrefabAddress { get; }
    }
}
