using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Weakness : IEffectDebuff
    {
        public Guid TypeId => new Guid("73b3fbd6-c647-4193-ad4e-fd6b4ffbc6f8");

        public string TypeName => nameof(Weakness);

        public bool IsSideEffect => false;
    }
}
