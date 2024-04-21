using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherHelmet : IArmorVisuals
    {
        private static readonly Guid Id = new Guid("b1b9b067-2523-4d57-a4c1-14b3a623f5f3");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Armor/Helm.prefab";

        public string ApplicableToTypeIdString => ArmorTypeIds.HelmId;
    }
}
