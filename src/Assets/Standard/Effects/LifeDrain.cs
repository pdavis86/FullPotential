using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects
{
    public class LifeDrain : IEffectDebuff
    {
        public Guid TypeId => new Guid("fe828022-b20d-4204-a6ff-8fada0e75bb5");

        public string TypeName => "Life Drain";

        public bool IsSideEffect => false;

    }
}
