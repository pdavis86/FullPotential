using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class ShoveLeft : IMovementEffect
    {
        public Guid TypeId => new Guid("997f2f31-0593-4db1-b0da-f985a4a2f17d");

        public string TypeName => nameof(ShoveLeft);

        public MovementDirection Direction => MovementDirection.LeftFromSource;
    }
}
