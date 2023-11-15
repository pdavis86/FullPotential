using System;
using FullPotential.Api.Registry.Shapes;

namespace FullPotential.Api.Gameplay.Shapes
{
    //todo: Why is Wall in API instead of Core?
    public class Wall : IShape
    {
        public const string TypeIdString = "4aad2866-5903-4b79-bda2-e3dcab920d9e";

        public Guid TypeId => new Guid(TypeIdString);

        public string TypeName => nameof(Wall);

        public string PrefabAddress => "Core/Prefabs/Shapes/Wall.prefab";

        public string VisualsFallbackPrefabAddress => "Core/Prefabs/Shapes/WallVisuals.prefab";
    }
}
