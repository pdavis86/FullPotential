using System;

using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Gameplay.Events
{
    public class BasicEventHandler<TArgs> : IEventHandler<TArgs>
        where TArgs : IEventHandlerArgs
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Func<TArgs, UniTask> BeforeHandlerAsync => null;

        public Func<TArgs, UniTask> AfterHandlerAsync { get; private set; }

        public BasicEventHandler(Func<TArgs, UniTask> basicFunction)
        {
            AfterHandlerAsync = basicFunction;
        }

        public BasicEventHandler(Action<TArgs> basicAction)
        {
            AfterHandlerAsync = args =>
            {
                basicAction(args);
                return UniTask.CompletedTask;
            };
        }
    }
}
