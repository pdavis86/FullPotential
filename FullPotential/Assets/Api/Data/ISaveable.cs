namespace FullPotential.Api.Data
{
    public interface ISaveable
    {
        bool IsDirty { get; set; }
    }
}
