using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class LeatherJerkin : IArmorVisuals
    {
        public Guid TypeId => new Guid("2419fcac-217e-48d8-9770-76c5ff27c9f8");

        public string TypeName => nameof(LeatherJerkin);

        public ArmorCategory Category => ArmorCategory.Chest;

        public string PrefabAddress => "Standard/Prefabs/Armor/Chest.prefab";
    }
}
