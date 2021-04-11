using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Debuffs
{
    public class ManaDrain : IEffectDebuff
    {
        public Guid TypeId => new Guid("e1ab10b2-fcae-4f25-a11f-ac5aeeadbdce");

        public string TypeName => nameof(ManaDrain);

        public bool IsSideEffect => false;
    }
}
