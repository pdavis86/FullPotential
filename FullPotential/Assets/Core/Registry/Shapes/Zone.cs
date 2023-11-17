using System;
using FullPotential.Api.Registry.Shapes;

namespace FullPotential.Core.Registry.Shapes
{
    public class Zone : IShape
    {
        public Guid TypeId => ShapeTypeIds.Zone;

        public string PrefabAddress => "Core/Prefabs/Shapes/Zone.prefab";

        public string VisualsFallbackPrefabAddress => "Core/Prefabs/Shapes/ZoneVisuals.prefab";
    }
}
