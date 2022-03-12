﻿using System;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Strengthen : IAttributeEffect
    {
        public Guid TypeId => new Guid("d1a00141-2014-43a3-8c2d-4422d93b8428");

        public string TypeName => nameof(Strengthen);

        public Affect Affect => Affect.TemporaryMaxIncrease;

        public string AttributeToAffect => nameof(Attributes.Strength);
    }
}
