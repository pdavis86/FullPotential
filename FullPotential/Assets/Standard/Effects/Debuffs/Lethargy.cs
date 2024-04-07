﻿using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Lethargy : IResourceEffect
    {
        private static readonly Guid Id = new Guid("260ff724-0708-42fa-81ab-45af911e6daf");

        public Guid TypeId => Id;

        public AffectType AffectType => AffectType.TemporaryMaxDecrease;

        public string ResourceTypeIdString => ResourceTypeIds.StaminaId;
    }
}
