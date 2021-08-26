using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Debuffs
{
    public class Poison : IEffectDebuff
    {
        public Guid TypeId => new Guid("756a664d-dcd5-4b01-9e42-bf2f6d2a9f0f");

        public string TypeName => nameof(Poison);

        public bool IsSideEffect => false;
    }
}
