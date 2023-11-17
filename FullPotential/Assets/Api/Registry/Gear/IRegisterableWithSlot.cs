namespace FullPotential.Api.Registry.Gear
{
    public interface IRegisterableWithSlot : IRegisterable
    {
        public string SlotSpritePrefabAddress { get; }
    }
}
