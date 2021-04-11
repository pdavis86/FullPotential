using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Support
{
    public class Heal : IEffectSupport
    {
        public Guid TypeId => new Guid("091e97b6-e3c1-4fa0-961c-cbf831e755b5");

        public string TypeName => nameof(Heal);

        public bool IsSideEffect => false;
    }
}
