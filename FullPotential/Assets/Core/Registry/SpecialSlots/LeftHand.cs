using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Core.Registry.SpecialSlots
{
    public class LeftHand : ISlotType
    {
        private static readonly Guid Id = new Guid(HandSlotIds.LeftHand.ToString());

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/LeftHand.png";
    }
}
