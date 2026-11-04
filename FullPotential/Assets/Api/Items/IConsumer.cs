using System.Collections.Generic;

using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.Visuals;

namespace FullPotential.Api.Items
{
    public interface IConsumer : ICombatItem, IValuePoolConsumer, IHasTargetingAndShape, IHasItemVisuals
    {
        List<IStoppable> Stoppables { get; }

        void StopStoppables();

        float GetProjectileSpeed();

        float GetRange();
    }
}
