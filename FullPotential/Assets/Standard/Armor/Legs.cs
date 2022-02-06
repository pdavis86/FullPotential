using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Legs : IGearArmor
    {
        public Guid TypeId => new Guid("b4ec0616-8a9c-4052-a318-482a305bb263");

        public string TypeName => nameof(Legs);

        public IGearArmor.ArmorCategory Category => IGearArmor.ArmorCategory.Legs;

        public string PrefabAddress => "Standard/Prefabs/Armor/Legs.prefab";
    }
}
