﻿using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects.Movement
{
    public class Punch : IMovementEffect
    {
        public Guid TypeId => new Guid("691aa12f-b59a-4e98-8aba-6dc28c7a5839");

        public MovementDirection Direction => MovementDirection.AwayFromSource;
    }
}
