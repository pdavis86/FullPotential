using System;

using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Lunge : IMovementEffectType
    {
        public Guid TypeId { get; } = new Guid("f5daeb39-3a24-4920-ae9e-589550bbc3b4");

        public MovementDirection Direction => MovementDirection.Forwards;
    }
}
