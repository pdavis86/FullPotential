using System;
using FullPotential.Api.Registry.Shapes;

namespace FullPotential.Core.Gameplay.Shapes
{
    public class Wall : IShape
    {
        public Guid TypeId => ShapeTypeIds.Wall;

        public string TypeName => nameof(Wall);

        public string PrefabAddress => "Core/Prefabs/Shapes/Wall.prefab";

        public string VisualsFallbackPrefabAddress => "Core/Prefabs/Shapes/WallVisuals.prefab";
    }
}
