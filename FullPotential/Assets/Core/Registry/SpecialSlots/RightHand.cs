using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;

namespace FullPotential.Core.Registry.SpecialSlots
{
    public class RightHand : IRegisterableWithSlot
    {
        private static readonly Guid Id = new Guid(HandSlotIds.RightHand);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/RightHand.png";
    }
}
