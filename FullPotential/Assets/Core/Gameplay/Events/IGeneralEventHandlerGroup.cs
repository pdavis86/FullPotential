using System.Collections.Generic;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public interface IGeneralEventHandlerGroup : IEventHandlerGroup
    {
        IEnumerable<IEventHandler> GetHandlers();
    }
}
