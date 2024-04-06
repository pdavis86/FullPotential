using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects
{
    public class Push : IMovementEffect
    {
        public const string Id = "691aa12f-b59a-4e98-8aba-6dc28c7a5839";

        public Guid TypeId => new Guid(Id);

        public MovementDirection Direction => MovementDirection.AwayFromSource;
    }
}
