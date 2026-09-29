using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler
    {
        NetworkLocation Location { get; }

        Timing Timing { get; }
    }

    public interface IEventHandler<TEvent> : IEventHandler
        where TEvent : IEvent
    {
        UniTask<HandlerResult> HandleEventAsync(TEvent eventArgs);
    }
}
