using System;
using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Standard.Armor
{
    public class Barrier : IGearArmor
    {
        public Guid TypeId => new Guid("17a6e875-cccd-46f0-b525-fe15cfdd8096");

        public string TypeName => nameof(Barrier);

        public IGearArmor.ArmorCategory Category => IGearArmor.ArmorCategory.Barrier;

        public string PrefabAddress => "Standard/Prefabs/Armor/Barrier.prefab";
    }
}
