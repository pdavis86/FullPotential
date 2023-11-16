namespace FullPotential.Api.Registry
{
    public interface IHasVisuals
    {
        string VisualsTypeId { get; }

        IVisuals Visuals { set; }
    }
}
