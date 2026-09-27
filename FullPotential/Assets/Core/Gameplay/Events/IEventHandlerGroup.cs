using System.Collections.Generic;

namespace FullPotential.Core.Gameplay.Events
{
    public interface IEventHandlerGroup
    {
        IEnumerable<T> GetHandlersOfType<T>();

        void Add(object handler);

        bool Remove(object handler);
    }
}
