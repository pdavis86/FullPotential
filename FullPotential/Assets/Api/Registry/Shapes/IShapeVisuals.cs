using System;

namespace FullPotential.Api.Registry.Shapes
{
    public interface IShapeVisuals : IVisuals
    {
        Guid ShapeTypeId { get; }
    }
}
