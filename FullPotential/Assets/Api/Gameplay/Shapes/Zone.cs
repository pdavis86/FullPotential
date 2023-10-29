using System;
using FullPotential.Api.Registry.Shapes;

namespace FullPotential.Api.Gameplay.Shapes
{
    public class Zone : IShape
    {
        public const string Id = "142aeb3b-84b1-43c6-ae91-388b0901fa52";

        public Guid TypeId => new Guid(Id);

        public string TypeName => nameof(Zone);

        public string PrefabAddress => "Core/Prefabs/Shapes/Zone.prefab";

        public string VisualsFallbackPrefabAddress => "Core/Prefabs/Shapes/ZoneVisuals.prefab";
    }
}
