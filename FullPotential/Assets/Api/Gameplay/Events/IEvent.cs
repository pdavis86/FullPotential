namespace FullPotential.Api.Gameplay.Events
{
    public interface IEvent
    {
        bool IsCancelled { get; set; }
    }
}
