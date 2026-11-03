using System;

using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Plummet : IMovementEffectType
    {
        public Guid TypeId { get; } = new Guid("ea49804b-1360-4073-ba25-054ac67d7103");

        public MovementDirection Direction => MovementDirection.Down;
    }
}
