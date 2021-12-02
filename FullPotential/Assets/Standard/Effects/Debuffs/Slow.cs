using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Slow : IEffectDebuff
    {
        public Guid TypeId => new Guid("1a082fdc-22cd-44d6-83eb-ea504370937a");

        public string TypeName => nameof(Slow);

        public bool IsSideEffect => false;
    }
}
