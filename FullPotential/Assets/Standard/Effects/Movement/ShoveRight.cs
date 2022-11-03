using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class ShoveRight : IMovementEffect
    {
        public Guid TypeId => new Guid("04db50b9-9053-45c7-858c-46a83a05331b");

        public string TypeName => nameof(ShoveRight);

        public MovementDirection Direction => MovementDirection.RightFromSource;
    }
}
