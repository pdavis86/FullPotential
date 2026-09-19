using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Events
{
    public class BasicEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Func<TEvent, UniTask> BeforeHandlerAsync => null;

        public Func<TEvent, UniTask> AfterHandlerAsync { get; private set; }

        // Resharper disable once UnusedMember.Global
        public BasicEventHandler(Func<TEvent, UniTask> basicFunction)
        {
            AfterHandlerAsync = basicFunction;
        }

        public BasicEventHandler(Action<TEvent> basicAction)
        {
            AfterHandlerAsync = args =>
            {
                basicAction(args);
                return UniTask.CompletedTask;
            };
        }
    }
}
