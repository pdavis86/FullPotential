using System;

using FullPotential.Api.Registry;
using FullPotential.Standard.Armor;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherBoots : IItemVisualsType
    {
        private static readonly Guid Id = new Guid("e42aefc3-2834-4f61-897f-5fb62d439b56");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Armor/Feet.prefab";

        public string ApplicableToTypeIdString => Feet.TypeIdString;
    }
}
