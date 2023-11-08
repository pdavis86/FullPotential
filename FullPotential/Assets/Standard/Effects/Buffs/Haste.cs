using System;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Haste : IAttributeEffect
    {
        public Guid TypeId => new Guid("bff9d019-bd6d-4971-8abe-6bb816199464");

        public string TypeName => nameof(Haste);

        public bool TemporaryMaxIncrease => true;

        public AttributeAffected AttributeAffectedToAffect => AttributeAffected.Speed;
    }
}
