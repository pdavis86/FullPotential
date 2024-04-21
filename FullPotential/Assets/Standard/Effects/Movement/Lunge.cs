using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Lunge : IMovementEffectType
    {
        private static readonly Guid Id = new Guid("f5daeb39-3a24-4920-ae9e-589550bbc3b4");

        public Guid TypeId => Id;

        public MovementDirection Direction => MovementDirection.Forwards;
    }
}
