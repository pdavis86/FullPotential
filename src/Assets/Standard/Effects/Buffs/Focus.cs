using Assets.ApiScripts.Crafting;
using System;

namespace Assets.Standard.Effects.Buffs
{
    public class Focus : IEffectBuff
    {
        public Guid TypeId => new Guid("d19cde18-e5dd-4fc2-b14f-da7daa5014d4");

        public string TypeName => nameof(Focus);

        public bool IsSideEffect => false;
    }
}
