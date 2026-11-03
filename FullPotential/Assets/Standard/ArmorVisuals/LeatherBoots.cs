using System;

using FullPotential.Api.Registry;
using FullPotential.Standard.Armor;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherBoots : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("e42aefc3-2834-4f61-897f-5fb62d439b56");

        public string PrefabAddress => "Standard/Prefabs/Armor/Feet.prefab";

        public string ApplicableToTypeIdString => Feet.TypeIdString;
    }
}
