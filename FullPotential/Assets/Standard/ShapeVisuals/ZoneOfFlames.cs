using System;
using FullPotential.Api.Registry.Shapes;

namespace FullPotential.Standard.ShapeVisuals
{
    public class ZoneOfFlames : IShapeVisuals
    {
        public Guid TypeId => new Guid("ac703f12-4d5c-4b76-98f9-045c10b40fd0");

        public string TypeName => nameof(ZoneOfFlames);

        public string PrefabAddress => "Standard/Prefabs/Shapes/Zone.prefab";

        public Guid ApplicableToTypeId => new Guid(Api.Gameplay.Shapes.Zone.TypeIdString);
    }
}
