using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Buffs
{
    public class ManaTap : IEffectBuff
    {
        public Guid TypeId => new Guid("06629efd-6a9b-4627-8b02-37e8dab135a1");

        public string TypeName => nameof(ManaTap);

        public bool IsSideEffect => false;
    }
}
