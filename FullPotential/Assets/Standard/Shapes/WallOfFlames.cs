using FullPotential.Api.Registry.Shapes;
using System;

namespace FullPotential.Standard.Shapes
{
    public class WallOfFlames : IShapeVisuals
    {
        //public static string NetworkPrefabAddress = "Standard/Prefabs/Shapes/Wall.prefab";

        public Guid TypeId => new Guid("b9055f98-b1f1-4991-a375-12de118430a8");

        public string TypeName => nameof(WallOfFlames);

        public string PrefabAddress => "Standard/Prefabs/Shapes/Wall.prefab";

        public Guid ShapeTypeId => new Guid(Api.Gameplay.Shapes.Wall.Id);
    }
}
