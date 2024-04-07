using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialSlots
{
    public class RangedWeaponReloaderSlot : IRegisterableWithSlot
    {
        public const string TypeIdString = "8413b572-99b4-4ad3-be9b-62f9b4609519";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Reloader.png";
    }
}
