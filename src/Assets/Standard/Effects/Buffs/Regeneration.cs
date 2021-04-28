using Assets.ApiScripts.Registry;
using System;

namespace Assets.Standard.Effects.Buffs
{
    public class Regeneration : IEffectBuff
    {
        public Guid TypeId => new Guid("158d1f0e-994d-49f7-a649-33ff336b8309");

        public string TypeName => nameof(Regeneration);

        public bool IsSideEffect => false;
    }
}
