using System;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Standard.Shapes;

namespace FullPotential.Standard.ShapeVisuals
{
    public class ZoneOfFlames : IShapeVisuals
    {
        public Guid TypeId => new Guid("ac703f12-4d5c-4b76-98f9-045c10b40fd0");

        public string PrefabAddress => "Standard/Prefabs/Shapes/ZoneVisuals.prefab";

        public Guid ApplicableToTypeId => new Guid(Zone.TypeIdString);
    }
}
