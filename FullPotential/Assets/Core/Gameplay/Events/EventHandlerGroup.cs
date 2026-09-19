using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class EventHandlerGroup<TEventArgs> : IEventHandlerGroup where TEventArgs : IEventArgs
    {
        public HashSet<IEventHandler<TEventArgs>> Handlers { get; } = new HashSet<IEventHandler<TEventArgs>>();

        public void Add(object handler)
        {
            Handlers.Add((IEventHandler<TEventArgs>)handler);
        }
        public bool Remove(object handler)
        {
            return Handlers.Remove((IEventHandler<TEventArgs>)handler);
        }
    }
}
