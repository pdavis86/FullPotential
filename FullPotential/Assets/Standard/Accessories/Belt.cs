using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Belt : IAccessory
    {
        public const string TypeIdString = "6d4bce60-dda6-4a88-82fd-c2b086065c8b";

        public Guid TypeId => new Guid(TypeIdString);

        public Dictionary<string, IEventHandler> EventHandlers => null;

        public int SlotCount => 1;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Belt.png";
    }
}
