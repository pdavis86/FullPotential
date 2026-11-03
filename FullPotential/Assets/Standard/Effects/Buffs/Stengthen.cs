using System;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Strengthen : IAttributeEffect
    {
        public Guid TypeId { get; } = new Guid("d1a00141-2014-43a3-8c2d-4422d93b8428");

        public bool IsTemporaryMaxIncrease => true;

        public AttributeTypeId AttributeId => Strength.Id;
    }
}
