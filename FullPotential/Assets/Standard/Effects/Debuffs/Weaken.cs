using System;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Weaken : IAttributeEffect
    {
        public Guid TypeId { get; } = new Guid("73b3fbd6-c647-4193-ad4e-fd6b4ffbc6f8");

        public bool IsTemporaryMaxIncrease => false;

        public AttributeTypeId AttributeId => Strength.Id;
    }
}
