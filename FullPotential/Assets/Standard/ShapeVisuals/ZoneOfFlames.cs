using System;

using FullPotential.Api.Registry.Visuals;

using FullPotential.Standard.Shapes;

namespace FullPotential.Standard.ShapeVisuals
{
    public class ZoneOfFlames : IItemVisualsType
    {
        public Guid TypeId { get; } = new Guid("ac703f12-4d5c-4b76-98f9-045c10b40fd0");

        public string PrefabAddress => "Standard/Prefabs/Shapes/ZoneVisuals.prefab";

        public string ApplicableToTypeIdString => Zone.TypeIdString;
    }
}
