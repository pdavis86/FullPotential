using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects
{
    public class Push : IMovementEffect
    {
        private static readonly Guid Id = new Guid(EffectTypeIds.PushId);

        public Guid TypeId => Id;

        public MovementDirection Direction => MovementDirection.AwayFromSource;
    }
}
