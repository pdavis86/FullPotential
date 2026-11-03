using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Shapes;

namespace FullPotential.Standard.ShapeVisuals
{
    public class WallOfFlames : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("b9055f98-b1f1-4991-a375-12de118430a8");

        public string PrefabAddress => "Standard/Prefabs/Shapes/WallVisuals.prefab";

        public string ApplicableToTypeIdString => Wall.TypeIdString;
    }
}
