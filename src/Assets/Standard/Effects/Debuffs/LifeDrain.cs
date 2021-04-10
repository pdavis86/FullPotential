using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Debuffs
{
    public class LifeDrain : IEffectDebuff
    {
        public const string Id = "fe828022-b20d-4204-a6ff-8fada0e75bb5";

        public Guid TypeId => new Guid(Id);

        public string TypeName => "Life Drain";

        public bool IsSideEffect => false;
    }
}
