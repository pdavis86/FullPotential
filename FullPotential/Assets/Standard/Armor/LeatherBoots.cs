using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class LeatherBoots : IArmorVisuals
    {
        public Guid TypeId => new Guid("645b2a9b-02df-4fb7-bea2-9b7a2a9c620b");

        public string TypeName => nameof(LeatherBoots);

        public ArmorType Type => ArmorType.Feet;

        public string PrefabAddress => "Standard/Prefabs/Armor/Feet.prefab";
    }
}
