﻿using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Accessories
{
    public class Amulet : IAccessory
    {
        public const string TypeIdString = "ddeafb61-0163-4888-b355-16a37d3a33b5";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public int SlotCount => 1;

        public string SlotSpritePrefabAddress => "Standard/UI/Equipment/Amulet.png";
    }
}
