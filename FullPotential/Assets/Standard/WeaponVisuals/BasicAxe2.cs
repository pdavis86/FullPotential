using System;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicAxe2 : IWeaponVisuals
    {
        public Guid TypeId => new Guid("ac8a55d8-6304-47d6-a35d-eecb0319d1ad");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Axe2.prefab";

        public Guid ApplicableToTypeId => new Guid(Axe.TypeIdString);
    }
}
