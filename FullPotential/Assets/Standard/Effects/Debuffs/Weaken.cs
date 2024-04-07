using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Weaken : IAttributeEffect
    {
        private static readonly Guid Id = new Guid("73b3fbd6-c647-4193-ad4e-fd6b4ffbc6f8");

        public Guid TypeId => Id;

        public bool TemporaryMaxIncrease => false;

        public AttributeAffected AttributeAffectedToAffect => AttributeAffected.Strength;
    }
}
