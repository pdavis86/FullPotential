using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.SpecialSlots
{
    public class BarrierSlot : IRegisterableWithSlot
    {
        public const string TypeIdString = "29adbef1-8fe2-47c2-8e91-da33ed83a6c7";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Barrier.png";
    }
}
