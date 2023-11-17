using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Ammo;

namespace FullPotential.Standard.Weapons
{
    public class Crossbow : IWeapon
    {
        public const string TypeIdString = "3d8be950-b8b0-44c6-ab84-1bf8434d67bd";

        public Guid TypeId => new Guid(TypeIdString);

        public bool IsDefensive => false;

        public Guid? AmmunitionTypeId => new Guid(Arrow.Id);

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;
    }
}
