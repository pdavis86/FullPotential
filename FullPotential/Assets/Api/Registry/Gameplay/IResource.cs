﻿using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Api.Registry.Gameplay
{
    public interface IResource : IRegisterable
    {
        Color Color { get; }

        bool IsCraftable { get; }

        string ItemInHandDefaultPrefab { get; }

        Action<LivingEntityBase> ReplenishBehaviour { get; }
    }
}
