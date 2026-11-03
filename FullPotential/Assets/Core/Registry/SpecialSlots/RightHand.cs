using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Core.Registry.SpecialSlots
{
    public class RightHand : ISlotType
    {
        public Guid TypeId { get; } = new Guid(HandSlotIds.RightHand.ToString());

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/RightHand.png";
    }
}
