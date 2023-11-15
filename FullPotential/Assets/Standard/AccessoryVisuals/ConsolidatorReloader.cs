using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Accessories;

namespace FullPotential.Standard.AccessoryVisuals
{
    public class ConsolidatorReloader : IAccessoryVisuals
    {
        public Guid TypeId => new Guid("0298c98c-d9db-4127-bd57-e3045340088f");

        public string TypeName => nameof(ConsolidatorReloader);

        public string PrefabAddress => null;

        public Guid ApplicableToTypeId => new Guid(RangedWeaponReloader.TypeIdString);
    }
}
