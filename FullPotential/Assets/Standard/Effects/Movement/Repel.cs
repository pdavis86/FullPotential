using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Effects.Movement
{
    public class Repel : IEffectMovement
    {
        public Guid TypeId => new Guid("82fbd978-4df8-4108-b1b6-b1452712f9c3");

        public string TypeName => nameof(Repel);

        public bool IsSideEffect => false;
    }
}
