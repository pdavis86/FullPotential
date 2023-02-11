using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Standard.Armor
{
    public class Feet : IGearArmor
    {
        public Guid TypeId => new Guid("645b2a9b-02df-4fb7-bea2-9b7a2a9c620b");

        public string TypeName => nameof(Feet);

        public ArmorCategory Category => ArmorCategory.Feet;

        public string PrefabAddress => "Standard/Prefabs/Armor/Feet.prefab";
    }
}
