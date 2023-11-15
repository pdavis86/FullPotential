using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Armor;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherHelmet : IArmorVisuals
    {
        public Guid TypeId => new Guid("b1b9b067-2523-4d57-a4c1-14b3a623f5f3");

        public string TypeName => nameof(LeatherHelmet);

        public string PrefabAddress => "Standard/Prefabs/Armor/Helm.prefab";

        public Guid ApplicableToTypeId => new Guid(Helm.TypeIdString);
    }
}
