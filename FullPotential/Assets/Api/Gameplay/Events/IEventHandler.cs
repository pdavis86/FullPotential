using System;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler
    {
        Action<IEventHandlerArgs> BeforeHandler { get; }

        Action<IEventHandlerArgs> AfterHandler { get; }
    }
}
