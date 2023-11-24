using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Barrier : IAccessory
    {
        public const string TypeIdString = "17a6e875-cccd-46f0-b525-fe15cfdd8096";

        public Guid TypeId => new Guid(TypeIdString);

        public Dictionary<string, IEventHandler> EventHandlers { get; } = new Dictionary<string, IEventHandler>
        {
            {FighterBase.EventIdDamageTaken, new BarrierEventHandler()}
        };

        public int SlotCount => 1;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Barrier.png";
    }
}
