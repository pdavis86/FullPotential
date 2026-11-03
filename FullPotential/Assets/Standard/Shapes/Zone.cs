using System;

using FullPotential.Api.Registry.Shapes;

namespace FullPotential.Standard.Shapes
{
    public class Zone : IShapeType
    {
        public const string TypeIdString = "142aeb3b-84b1-43c6-ae91-388b0901fa52";
        public const string AddressablePath = "Standard/Prefabs/Shapes/Zone.prefab";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public string NetworkPrefabAddress => AddressablePath;
    }
}
