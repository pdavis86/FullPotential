namespace FullPotential.Api.Registry
{
    public interface IHasItemVisuals
    {
        string VisualsTypeId { get; }

        IItemVisuals Visuals { set; }
    }
}
