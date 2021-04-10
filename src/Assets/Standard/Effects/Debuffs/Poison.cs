using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Debuffs
{
    public class Poison : IEffectDebuff
    {
        public const string Id = "756a664d-dcd5-4b01-9e42-bf2f6d2a9f0f";

        public Guid TypeId => new Guid(Id);

        public string TypeName => nameof(Poison);

        public bool IsSideEffect => false;
    }
}
