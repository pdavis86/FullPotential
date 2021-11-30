using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Armor
{
    public class Legs : IGearArmor
    {
        public Guid TypeId => new Guid("b4ec0616-8a9c-4052-a318-482a305bb263");

        public string TypeName => nameof(Legs);

        public IGearArmor.ArmorCategory Category => IGearArmor.ArmorCategory.Legs;

        //todo: missing prefab
        public string PrefabAddress => throw new NotImplementedException();
    }
}
