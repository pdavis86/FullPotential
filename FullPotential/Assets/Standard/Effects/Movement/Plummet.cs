using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Plummet : IMovementEffect
    {
        private static readonly Guid Id = new Guid("ea49804b-1360-4073-ba25-054ac67d7103");

        public Guid TypeId => Id;

        public MovementDirection Direction => MovementDirection.Down;
    }
}
