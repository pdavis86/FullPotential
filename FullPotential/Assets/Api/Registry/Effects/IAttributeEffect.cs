﻿using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Effects
{
    public interface IAttributeEffect : IEffect
    {
        /// <summary>
        /// Whether the maximum will be temporarily increased (otherwise decreased)
        /// </summary>
        bool TemporaryMaxIncrease { get; }

        /// <summary>
        /// The attribute which will be affected
        /// </summary>
        AttributeAffected AttributeAffectedToAffect { get; }
    }
}
