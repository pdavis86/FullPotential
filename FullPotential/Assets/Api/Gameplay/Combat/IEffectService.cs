﻿using FullPotential.Api.Registry.Base;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IEffectService
    {
        void ApplyEffects(
            GameObject source,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position
        );
    }
}