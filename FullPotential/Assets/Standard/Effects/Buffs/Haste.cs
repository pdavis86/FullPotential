using System;

using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Gameplay.Attributes;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Haste : IAttributeEffect
    {
        private static readonly Guid Id = new Guid("bff9d019-bd6d-4971-8abe-6bb816199464");

        public Guid TypeId => Id;

        public bool IsTemporaryMaxIncrease => true;

        public string AttributeId => Speed.TypeIdString;
    }
}
