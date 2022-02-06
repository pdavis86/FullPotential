using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Launch : IEffectMovement
    {
        public Guid TypeId => new Guid("a2cb2a03-3684-450d-a1cb-6396dc96ab48");

        public string TypeName => nameof(Launch);

        public bool IsSideEffect => false;
    }
}
