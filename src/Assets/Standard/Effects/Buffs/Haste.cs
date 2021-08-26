using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Buffs
{
    public class Haste : IEffectBuff
    {
        public Guid TypeId => new Guid("bff9d019-bd6d-4971-8abe-6bb816199464");

        public string TypeName => nameof(Haste);

        public bool IsSideEffect => false;
    }
}
