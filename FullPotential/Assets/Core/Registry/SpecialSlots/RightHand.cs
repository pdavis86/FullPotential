using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;

namespace FullPotential.Core.Registry.SpecialSlots
{
    public class RightHand : IRegisterableWithSlot
    {
        public Guid TypeId => new Guid(HandSlotIds.RightHand);

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/RightHand.png";
    }
}
