using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;

namespace FullPotential.Core.Registry.SpecialSlots
{
    //todo: is this needed?
    public class RightHand : IRegisterableWithSlot
    {
        public Guid TypeId => new Guid(SlotIds.RightHand);

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/RightHand.png";
    }
}
