using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;

namespace FullPotential.Core.Registry.SpecialSlots
{
    public class LeftHand : IRegisterableWithSlot
    {
        public Guid TypeId => new Guid(HandSlotIds.LeftHand);

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/LeftHand.png";
    }
}
