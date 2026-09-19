namespace FullPotential.Api.Gameplay.Events
{
    public interface IEvent
    {
        bool IsDefaultHandlerCancelled { get; set; }
    }
}
