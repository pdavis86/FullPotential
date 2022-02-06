﻿using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class Attract : IEffectMovement
    {
        public Guid TypeId => new Guid("0e67f9ac-ef90-467e-ba7e-a4af3d965baa");

        public string TypeName => nameof(Attract);

        public bool IsSideEffect => false;
    }
}
