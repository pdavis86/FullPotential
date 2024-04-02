using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ShortCircuit : IResourceEffect
    {
        public Guid TypeId => new Guid("434fc4a7-dcbc-44b8-86d6-5815a42eea0b");

        public AffectType AffectType => AffectType.TemporaryMaxDecrease;

        public Guid ResourceTypeId => Energy.Id;
    }
}
