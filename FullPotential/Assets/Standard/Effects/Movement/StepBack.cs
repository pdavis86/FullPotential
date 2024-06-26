﻿using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Movement
{
    public class StepBack : IMovementEffect
    {
        private static readonly Guid Id = new Guid("90781986-e490-4115-a7f6-7864c3ed1e41");

        public Guid TypeId => Id;

        public MovementDirection Direction => MovementDirection.Backwards;
    }
}
