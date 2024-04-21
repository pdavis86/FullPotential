using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Attract : IMovementEffectType
    {
        private static readonly Guid Id = new Guid("0e67f9ac-ef90-467e-ba7e-a4af3d965baa");

        public Guid TypeId => Id;

        public MovementDirection Direction => MovementDirection.TowardSource;
    }
}
