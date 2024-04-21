using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Hold : IMovementEffectType
    {
        private static readonly Guid Id = new Guid("3ff5300f-fc87-43ae-9e08-2a9a8fa54813");

        public Guid TypeId => Id;

        public MovementDirection Direction => MovementDirection.MaintainDistance;
    }
}
