using Assets.ApiScripts.Registry;
using System;

namespace Assets.Standard.Effects.Debuffs
{
    public class Fear : IEffectDebuff
    {
        public Guid TypeId => new Guid("cc67431d-09d7-4c62-9fde-2dc5aa596ac7");

        public string TypeName => nameof(Fear);

        public bool IsSideEffect => false;
    }
}
