using System;
using FullPotential.Api.Registry.Weapons;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Standard.Weapons.Ammo
{
    public class Arrow : IAmmunition
    {
        internal const string Id = "d84c2b09-f6b2-48cf-a781-c1025cdc56be";

        public Guid TypeId => new Guid(Id);

        public string TypeName => nameof(Arrow);

        public int MaxStackSize => 15;

        public int MinDropCount => 1;

        public int MaxDropCount => 5;
    }
}
