using System;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler
    {
        Action<IEventHandlerArgs> BeforeEvent { get; }

        Action<IEventHandlerArgs> AfterEvent { get; }
    }
}
