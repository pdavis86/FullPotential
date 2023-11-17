using System;
using FullPotential.Api.Registry.Weapons;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Standard.Ammo
{
    public class Arrow : IAmmunition
    {
        internal const string Id = "d84c2b09-f6b2-48cf-a781-c1025cdc56be";

        public Guid TypeId => new Guid(Id);

        public int MaxStackSize => 50;

        public int MinDropCount => 20;

        public int MaxDropCount => 40;
    }
}
