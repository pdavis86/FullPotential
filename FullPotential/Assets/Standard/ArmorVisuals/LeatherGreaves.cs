using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherGreaves : IArmorVisuals
    {
        private static readonly Guid Id = new Guid("4eda8bc2-6929-4ad6-a5e1-3103b2cbcdac");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Armor/Legs.prefab";

        public string ApplicableToTypeIdString => ArmorTypeIds.LegsId;
    }
}
