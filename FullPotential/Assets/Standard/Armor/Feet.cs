using System;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class Feet : IGearArmor
    {
        public Guid TypeId => new Guid("645b2a9b-02df-4fb7-bea2-9b7a2a9c620b");

        public string TypeName => nameof(Feet);

        public IGearArmor.ArmorCategory Category => IGearArmor.ArmorCategory.Feet;

        public string PrefabAddress => "Standard/Prefabs/Armor/Feet.prefab";
    }
}
