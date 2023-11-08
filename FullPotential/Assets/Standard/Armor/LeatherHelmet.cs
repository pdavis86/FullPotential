using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class LeatherHelmet : IArmorVisuals
    {
        public Guid TypeId => new Guid("bd6f655b-6fed-42e5-8797-e9cb3f675696");

        public string TypeName => nameof(LeatherHelmet);

        public ArmorType Type => ArmorType.Helm;

        public string PrefabAddress => "Standard/Prefabs/Armor/Helm.prefab";
    }
}
