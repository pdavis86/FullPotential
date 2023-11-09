namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }
    }
}
