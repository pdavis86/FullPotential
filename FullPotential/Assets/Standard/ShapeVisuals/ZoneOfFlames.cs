using System;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Standard.Shapes;

namespace FullPotential.Standard.ShapeVisuals
{
    public class ZoneOfFlames : IShapeVisuals
    {
        private static readonly Guid Id = new Guid("ac703f12-4d5c-4b76-98f9-045c10b40fd0");

        public Guid TypeId => Id;

        public string PrefabAddress => "Standard/Prefabs/Shapes/ZoneVisuals.prefab";

        public string ApplicableToTypeIdString => Zone.TypeIdString;
    }
}
