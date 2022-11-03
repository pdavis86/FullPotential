using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Hold : IMovementEffect
    {
        public Guid TypeId => new Guid("3ff5300f-fc87-43ae-9e08-2a9a8fa54813");

        public string TypeName => nameof(Hold);

        public MovementDirection Direction => MovementDirection.MaintainDistance;
    }
}
