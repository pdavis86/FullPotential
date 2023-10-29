namespace FullPotential.Api.Registry.Shapes
{
    public interface IShape : IRegisterable, IHasPrefab
    {
        string VisualsFallbackPrefabAddress { get; }
    }
}
