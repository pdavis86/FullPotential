using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Launch : IMovementEffectType
    {
        private static readonly Guid Id = new Guid("a2cb2a03-3684-450d-a1cb-6396dc96ab48");

        public Guid TypeId => Id;

        public MovementDirection Direction => MovementDirection.Up;
    }
}
