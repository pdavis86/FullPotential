using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Core.Registry.SpecialSlots
{
    public class LeftHand : ISlotType
    {
        public Guid TypeId { get; } = new Guid(HandSlotIds.LeftHand.ToString());

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/LeftHand.png";
    }
}
