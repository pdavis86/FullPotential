using System;
using FullPotential.Api.Registry.Shapes;

namespace FullPotential.Standard.Shapes
{
    public class Wall : IShape
    {
        public const string TypeIdString = "4aad2866-5903-4b79-bda2-e3dcab920d9e";
        public const string AddressablePath = "Standard/Prefabs/Shapes/Wall.prefab";

        public Guid TypeId => new Guid(TypeIdString);

        public string NetworkPrefabAddress => AddressablePath;
    }
}
