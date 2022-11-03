using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Plummet : IMovementEffect
    {
        public Guid TypeId => new Guid("ea49804b-1360-4073-ba25-054ac67d7103");

        public string TypeName => nameof(Plummet);

        public MovementDirection Direction => MovementDirection.Down;
    }
}
