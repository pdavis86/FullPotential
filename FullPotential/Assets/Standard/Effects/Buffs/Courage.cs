using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Courage : IEffectBuff
    {
        public Guid TypeId => new Guid("ee5271a8-ef14-4f2a-b34b-5ae5a091520f");

        public string TypeName => nameof(Courage);

        public bool IsSideEffect => false;
    }
}
