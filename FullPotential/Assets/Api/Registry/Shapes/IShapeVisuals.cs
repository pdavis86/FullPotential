using System;
using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Api.Registry.Shapes
{
    public interface IShapeVisuals : IRegisterable, IHasPrefab
    {
        Guid ShapeGuid { get; }
    }
}
