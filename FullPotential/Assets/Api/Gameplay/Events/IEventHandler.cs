using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler<TEventArgs> where TEventArgs : IEventArgs
    {
        NetworkLocation Location { get; }

        Timing Timing { get; }

        UniTask<HandlerResult> HandleEventAsync(TEventArgs eventArgs);
    }
}
