﻿using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Fear : IStatEffect
    {
        public Guid TypeId => new Guid("cc67431d-09d7-4c62-9fde-2dc5aa596ac7");

        public string TypeName => nameof(Fear);

        public AffectType AffectType => AffectType.TemporaryMaxDecrease;

        public AffectableStat StatToAffect => AffectableStat.Health;
    }
}
