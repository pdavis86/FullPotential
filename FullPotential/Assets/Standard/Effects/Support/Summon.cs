using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Summon : IEffect
    {
        public Guid TypeId => new Guid("668f5ab8-0f8d-4f8c-8e88-90eb7367d22a");

        public string TypeName => nameof(Summon);

        public Affect Affect => Affect.ConjureAlly;
    }
}
