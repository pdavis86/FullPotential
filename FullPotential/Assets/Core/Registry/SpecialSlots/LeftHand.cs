using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;

namespace FullPotential.Core.Registry.SpecialSlots
{
    //todo: is this needed?
    public class LeftHand : IRegisterableWithSlot
    {
        public Guid TypeId => new Guid(SlotIds.LeftHand);

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/LeftHand.png";
    }
}
