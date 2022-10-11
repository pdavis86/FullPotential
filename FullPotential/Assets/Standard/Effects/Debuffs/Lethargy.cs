﻿using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Lethargy : IStatEffect
    {
        public Guid TypeId => new Guid("260ff724-0708-42fa-81ab-45af911e6daf");

        public string TypeName => nameof(Lethargy);

        public Affect Affect => Affect.TemporaryMaxDecrease;

        public AffectableStat StatToAffect => AffectableStat.Stamina;
    }
}