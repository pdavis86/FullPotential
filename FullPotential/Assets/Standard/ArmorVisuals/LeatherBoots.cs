using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherBoots : IArmorVisuals
    {
        private static readonly Guid Id = new Guid("e42aefc3-2834-4f61-897f-5fb62d439b56");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Armor/Feet.prefab";

        public string ApplicableToTypeIdString => ArmorTypeIds.FeetId;
    }
}
