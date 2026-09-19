namespace FullPotential.Api.Registry.Gear
{
    public interface IRegisterableWithSlotType : IRegisterableType
    {
        string SlotSpritePrefabAddress { get; }
    }
}
