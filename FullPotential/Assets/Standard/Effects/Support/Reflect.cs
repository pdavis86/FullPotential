using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Reflect : ICustomEffectType
    {
        private static readonly Guid Id = new Guid("7e9f4b8a-2c5f-41cb-b585-e06ab59d2277");

        public Guid TypeId => Id;

        public void ApplyEffect()
        {
            throw new NotImplementedException();
        }
    }
}
