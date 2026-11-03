using System;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Haste : IAttributeEffect
    {
        public Guid TypeId { get; } = new Guid("bff9d019-bd6d-4971-8abe-6bb816199464");

        public bool IsTemporaryMaxIncrease => true;

        public AttributeTypeId AttributeId => Speed.Id;
    }
}
