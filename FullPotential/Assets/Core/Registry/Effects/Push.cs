using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects
{
    public class Push : IMovementEffectType
    {
        public Guid TypeId { get; } = new Guid(EffectTypeIds.PushId);

        public MovementDirection Direction => MovementDirection.AwayFromSource;
    }
}
