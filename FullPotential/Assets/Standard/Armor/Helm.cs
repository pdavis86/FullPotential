using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Armor
{
    public class Helm : IGearArmor
    {
        public Guid TypeId => new Guid("bd6f655b-6fed-42e5-8797-e9cb3f675696");

        public string TypeName => nameof(Helm);

        public IGearArmor.ArmorCategory Category => IGearArmor.ArmorCategory.Helm;

        //todo: missing prefab
        public string PrefabAddress => throw new NotImplementedException();
    }
}
