using FullPotential.Api.Registry.Shapes;
using System;

namespace FullPotential.Standard.Shapes
{
    public class ZoneVisuals : IShapeVisuals
    {
        public Guid TypeId => new Guid("ac703f12-4d5c-4b76-98f9-045c10b40fd0");

        public string TypeName => nameof(ZoneVisuals);

        public string PrefabAddress => "Standard/Prefabs/Shapes/Zone.prefab";

        public Guid ShapeTypeId => new Guid(Api.Gameplay.Shapes.Zone.Id);
    }
}
