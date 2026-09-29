namespace FullPotential.Api.Gameplay.Events
{
    public interface IScopedEventHandler
    {
        object Owner { get; }
    }
}
