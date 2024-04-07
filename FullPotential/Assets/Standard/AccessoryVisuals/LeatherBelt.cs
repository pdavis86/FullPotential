using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Accessories;

namespace FullPotential.Standard.AccessoryVisuals
{
    public class LeatherBelt : IAccessoryVisuals
    {
        private static readonly Guid Id = new Guid("3275abb7-fc48-4682-abc1-85dda7abf24e");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Accessories/Belt.prefab";

        public string ApplicableToTypeIdString => Belt.TypeIdString;
    }
}
