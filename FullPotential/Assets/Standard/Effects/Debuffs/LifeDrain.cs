using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Debuffs
{
    public class LifeDrain : IEffectDebuff
    {
        public Guid TypeId => new Guid("fe828022-b20d-4204-a6ff-8fada0e75bb5");

        public string TypeName => nameof(LifeDrain);

        public bool IsSideEffect => false;
    }
}
