using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Core.Registry.SpecialSlots
{
    public class RightHand : ISlotType
    {
        private static readonly Guid Id = new Guid(HandSlotIds.RightHand.ToString());

        public Guid TypeId => Id;

        public string SlotSpritePrefabAddress => "Core/UI/Equipment/RightHand.png";
    }
}
