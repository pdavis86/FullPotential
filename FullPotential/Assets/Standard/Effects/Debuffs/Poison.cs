﻿using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Poison : IResourceEffect
    {
        private static readonly Guid Id = new Guid("756a664d-dcd5-4b01-9e42-bf2f6d2a9f0f");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.PeriodicDecrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;
    }
}
