using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Debuffs
{
    public class Weaken : IEffectDebuff
    {
        public Guid TypeId => new Guid("73b3fbd6-c647-4193-ad4e-fd6b4ffbc6f8");

        public string TypeName => nameof(Weaken);

        public bool IsSideEffect => false;
    }
}
