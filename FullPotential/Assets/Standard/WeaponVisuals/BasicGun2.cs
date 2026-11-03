using System;

using FullPotential.Api.Registry;
using FullPotential.Standard.Weapons;

namespace FullPotential.Standard.WeaponVisuals
{
    public class BasicGun2 : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("afa3e371-9fd0-4081-ba5f-bf2c922cd87d");

        public string PrefabAddress => "Standard/Prefabs/Weapons/Gun2.prefab";

        public string ApplicableToTypeIdString => Gun.TypeIdString;
    }
}
