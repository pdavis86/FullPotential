using System;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler
    {
        NetworkLocation Location { get; }

        Action<IEventHandlerArgs> BeforeHandler { get; }

        Action<IEventHandlerArgs> AfterHandler { get; }
    }
}
