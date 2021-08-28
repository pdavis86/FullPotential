﻿using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Weapons
{
    public class Crossbow : IGearWeapon
    {
        public Guid TypeId => new Guid("3d8be950-b8b0-44c6-ab84-1bf8434d67bd");

        public string TypeName => nameof(Crossbow);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Ranged;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => true;

        public string PrefabAddress => null;

        public string PrefabAddressTwoHanded => "Standard/Prefabs/Crossbow.prefab";
    }
}