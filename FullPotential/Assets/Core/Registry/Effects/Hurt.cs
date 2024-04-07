using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Effects
{
    public class Hurt : IResourceEffect
    {
        public Guid TypeId => new Guid(EffectTypeIds.HurtId);

        public AffectType AffectType => AffectType.SingleDecrease;

        //todo: when referencing other Guids, use string. Then remove all public static readonly Guid
        public Guid ResourceTypeId => ResourceTypeIds.Health;
    }
}
