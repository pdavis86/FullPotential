using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Accessories;

namespace FullPotential.Standard.AccessoryVisuals
{
    public class LeatherBelt : IAccessoryVisuals
    {
        public Guid TypeId => new Guid("3275abb7-fc48-4682-abc1-85dda7abf24e");

        public string PrefabAddress => "Standard/Prefabs/Accessories/Belt.prefab";

        public Guid ApplicableToTypeId => new Guid(Belt.TypeIdString);
    }
}
