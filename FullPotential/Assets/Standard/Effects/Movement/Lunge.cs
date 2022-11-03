using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Lunge : IMovementEffect
    {
        public Guid TypeId => new Guid("f5daeb39-3a24-4920-ae9e-589550bbc3b4");

        public string TypeName => nameof(Lunge);

        public MovementDirection Direction => MovementDirection.Forwards;
    }
}
