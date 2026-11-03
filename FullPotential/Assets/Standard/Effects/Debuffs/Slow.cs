using System;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Slow : IAttributeEffect
    {
        public Guid TypeId { get; } = new Guid("1a082fdc-22cd-44d6-83eb-ea504370937a");

        public bool IsTemporaryMaxIncrease => false;

        public AttributeTypeId AttributeId => Speed.Id;
    }
}
