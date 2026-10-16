using System;

using FullPotential.Api.Registry;
using FullPotential.Standard.Armor;

namespace FullPotential.Standard.ArmorVisuals
{
    public class LeatherJerkin : IItemVisualsType
    {
        private static readonly Guid Id = new Guid("a2989e18-6830-4770-8695-1c8592137e2d");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Armor/Chest.prefab";

        public string ApplicableToTypeIdString => Chest.TypeIdString;
    }
}
