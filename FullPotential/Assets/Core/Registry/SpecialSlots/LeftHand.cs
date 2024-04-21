using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;

namespace FullPotential.Core.Registry.SpecialSlots
{
    public class LeftHand : IRegisterableWithSlotType
    {
        private static readonly Guid Id = new Guid(HandSlotIds.LeftHand);

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/LeftHand.png";
    }
}
