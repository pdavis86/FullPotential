using Cysharp.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventManager
    {
        void Subscribe<T>(string eventId) where T : IEventHandler;

        UniTask TriggerAsync(string eventId, IEventHandlerArgs args);
    }
}
