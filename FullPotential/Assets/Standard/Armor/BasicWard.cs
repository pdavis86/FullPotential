using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Standard.Armor
{
    public class BasicWard : IArmorVisuals
    {
        public Guid TypeId => new Guid("17a6e875-cccd-46f0-b525-fe15cfdd8096");

        public string TypeName => nameof(BasicWard);

        public ArmorType Type => ArmorType.Barrier;

        public string PrefabAddress => "Standard/Prefabs/Armor/Barrier.prefab";
    }
}
