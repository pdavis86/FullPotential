using System;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Slow : IAttributeEffect
    {
        public Guid TypeId => new Guid("1a082fdc-22cd-44d6-83eb-ea504370937a");

        public string TypeName => nameof(Slow);

        public Affect Affect => Affect.TemporaryMaxDecrease;

        public string AttributeToAffect => nameof(Attributes.Speed);
    }
}
