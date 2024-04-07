using System;
using FullPotential.Api.Registry.Weapons;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Standard.Ammo
{
    public class Arrow : IAmmunition
    {
        public const string TypeIdString = "d84c2b09-f6b2-48cf-a781-c1025cdc56be";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public int MaxStackSize => 50;

        public int MinDropCount => 20;

        public int MaxDropCount => 40;
    }
}
