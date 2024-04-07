using System;
using FullPotential.Api.Registry.Weapons;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Standard.Ammo
{
    public class Bullet : IAmmunition
    {
        public const string TypeIdString = "130cc39d-e7db-4b8b-bb09-53a5d68fa05d";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public int MaxStackSize => 500;

        public int MinDropCount => 100;

        public int MaxDropCount => 400;
    }
}
