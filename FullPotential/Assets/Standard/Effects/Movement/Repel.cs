using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Repel : IMovementEffect
    {
        private static readonly Guid Id = new Guid("82fbd978-4df8-4108-b1b6-b1452712f9c3");

        public Guid TypeId => Id;

        public MovementDirection Direction => MovementDirection.AwayFromSource;
    }
}
