using System;
using FullPotential.Api.Registry.Armor;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherBoots : IArmorVisuals
    {
        public Guid TypeId => new Guid("e42aefc3-2834-4f61-897f-5fb62d439b56");

        public string PrefabAddress => "Standard/Prefabs/Armor/Feet.prefab";

        public Guid ApplicableToTypeId => ArmorTypeIds.Feet;
    }
}
