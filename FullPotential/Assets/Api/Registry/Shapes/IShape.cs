namespace FullPotential.Api.Registry.Shapes
{
    public interface IShape : IRegisterable
    {
        string VisualsFallbackPrefabAddress { get; }
    }
}
