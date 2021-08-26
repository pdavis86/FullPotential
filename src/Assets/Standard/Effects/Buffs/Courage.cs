using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Buffs
{
    public class Courage : IEffectBuff
    {
        public Guid TypeId => new Guid("ee5271a8-ef14-4f2a-b34b-5ae5a091520f");

        public string TypeName => nameof(Courage);

        public bool IsSideEffect => false;
    }
}
