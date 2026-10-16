using Cysharp.Threading.Tasks;

namespace FullPotential.Api.EventManagement
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
