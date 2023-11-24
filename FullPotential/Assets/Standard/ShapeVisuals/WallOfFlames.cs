using System;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Standard.Shapes;

namespace FullPotential.Standard.ShapeVisuals
{
    public class WallOfFlames : IShapeVisuals
    {
        public Guid TypeId => new Guid("b9055f98-b1f1-4991-a375-12de118430a8");

        public string PrefabAddress => "Standard/Prefabs/Shapes/WallVisuals.prefab";

        public Guid ApplicableToTypeId => new Guid(Wall.TypeIdString);
    }
}
