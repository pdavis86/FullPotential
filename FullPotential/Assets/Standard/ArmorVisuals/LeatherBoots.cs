using System;
using FullPotential.Api.Registry.Gear;
using FullPotential.Standard.Armor;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherBoots : IArmorVisuals
    {
        public Guid TypeId => new Guid("e42aefc3-2834-4f61-897f-5fb62d439b56");

        public string TypeName => nameof(LeatherBoots);

        public string PrefabAddress => "Standard/Prefabs/Armor/Feet.prefab";

        public Guid ApplicableToTypeId => new Guid(Feet.TypeIdString);
    }
}
