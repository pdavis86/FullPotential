using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects
{
    public class Push : IMovementEffect
    {
        public Guid TypeId => new Guid(EffectTypeIds.PushId);

        public MovementDirection Direction => MovementDirection.AwayFromSource;
    }
}
