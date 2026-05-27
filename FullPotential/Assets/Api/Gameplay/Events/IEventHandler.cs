using System;

using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler<TArgs> where TArgs : IEventHandlerArgs
    {
        NetworkLocation Location { get; }

        Func<TArgs, UniTask> BeforeHandlerAsync { get; }

        Func<TArgs, UniTask> AfterHandlerAsync { get; }
    }
}
