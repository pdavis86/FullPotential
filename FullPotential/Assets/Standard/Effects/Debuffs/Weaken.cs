using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Weaken : IAttributeEffect
    {
        public Guid TypeId => new Guid("73b3fbd6-c647-4193-ad4e-fd6b4ffbc6f8");

        public string TypeName => nameof(Weaken);

        public bool TemporaryMaxIncrease => false;

        public AffectableAttribute AttributeToAffect => AffectableAttribute.Strength;
    }
}
