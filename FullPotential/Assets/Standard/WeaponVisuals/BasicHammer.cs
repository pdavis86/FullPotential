﻿using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicHammer : IWeaponVisuals
    {
        private static readonly Guid Id = new Guid("f729f525-634a-40ec-b51b-6590039a9d0c");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Weapons/Hammer.prefab";

        public string ApplicableToTypeIdString => Hammer.TypeIdString;
    }
}
