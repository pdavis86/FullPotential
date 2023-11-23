using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Fear : IResourceEffect
    {
        public Guid TypeId => new Guid("cc67431d-09d7-4c62-9fde-2dc5aa596ac7");

        public AffectType AffectType => AffectType.TemporaryMaxDecrease;

        public Guid ResourceTypeId => ResourceTypeIds.Health;
    }
}
