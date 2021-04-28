using Assets.ApiScripts.Registry;
using System;

namespace Assets.Standard.Effects.Buffs
{
    public class Strength : IEffectBuff
    {
        public Guid TypeId => new Guid("d1a00141-2014-43a3-8c2d-4422d93b8428");

        public string TypeName => nameof(Strength);

        public bool IsSideEffect => false;
    }
}
