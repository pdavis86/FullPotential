using Assets.ApiScripts.Registry;
using System;

namespace Assets.Standard.Effects.Buffs
{
    public class Haste : IEffectBuff
    {
        public Guid TypeId => new Guid("bff9d019-bd6d-4971-8abe-6bb816199464");

        public string TypeName => nameof(Haste);

        public bool IsSideEffect => false;
    }
}
