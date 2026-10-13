using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherHelmet : IItemVisuals
    {
        private static readonly Guid Id = new Guid("b1b9b067-2523-4d57-a4c1-14b3a623f5f3");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Armor/Helm.prefab";

        public string ApplicableToTypeIdString => ArmorTypeIds.HelmId;
    }
}
