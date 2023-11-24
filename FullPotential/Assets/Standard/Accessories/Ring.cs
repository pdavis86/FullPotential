﻿using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Ring : IAccessory
    {
        public const string TypeIdString = "b74b00f9-9cf1-4758-9e22-b4fbd4d1cea0";

        public Guid TypeId => new Guid(TypeIdString);

        public Dictionary<string, IEventHandler> EventHandlers => null;
        
        public int SlotCount => 2;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Ring.png";
    }
}
