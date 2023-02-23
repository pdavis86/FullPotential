using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Api.Registry.Shapes
{
    public interface IShapeVisuals<T> : IRegisterable, IHasPrefab
        where T : IShape
    {
    }
}
