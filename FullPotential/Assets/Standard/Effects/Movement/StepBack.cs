using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class StepBack : IMovementEffect
    {
        public Guid TypeId => new Guid("90781986-e490-4115-a7f6-7864c3ed1e41");

        public string TypeName => nameof(StepBack);

        public MovementDirection Direction => MovementDirection.Backwards;
    }
}
