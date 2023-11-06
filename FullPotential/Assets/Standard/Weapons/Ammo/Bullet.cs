﻿using System;
using FullPotential.Api.Registry.Weapons;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Standard.Weapons.Ammo
{
    public class Bullet : IAmmunition
    {
        internal const string Id = "130cc39d-e7db-4b8b-bb09-53a5d68fa05d";

        public Guid TypeId => new Guid(Id);

        public string TypeName => nameof(Bullet);

        public int MaxStackSize => 50;

        public int MinDropCount => 10;

        public int MaxDropCount => 30;
    }
}
