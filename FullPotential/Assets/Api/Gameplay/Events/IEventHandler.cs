using System;

using Cysharp.Threading.Tasks;

namespace FullPotential.Api.Gameplay.Events
{
    public interface IEventHandler
    {
        NetworkLocation Location { get; }

        Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync { get; }

        Func<IEventHandlerArgs, UniTask> AfterHandlerAsync { get; }
    }
}
