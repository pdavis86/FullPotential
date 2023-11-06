﻿using System;

namespace FullPotential.Api.Registry.Weapons
{
    public interface IWeapon : IRegisterable, IHasPrefab
    {
        /// <summary>
        /// This weapon is used primarily for defense
        /// </summary>
        bool IsDefensive { get; }

        /// <summary>
        /// Optional, what type of ammunition is used
        /// </summary>
        Guid? AmmunitionTypeId { get; }

        /// <summary>
        /// Whether the weapon has the capability of automatic fire
        /// </summary>
        bool AllowAutomatic { get; }

        /// <summary>
        /// Whether the weapon has a two-handed variant
        /// </summary>
        bool AllowTwoHanded { get; }

        /// <summary>
        /// Whether the weapon can only be a two-handed variant
        /// </summary>
        bool EnforceTwoHanded { get; }

        /// <summary>
        /// The address of the prefab to load for the two-handed version of this weapon
        /// </summary>
        string PrefabAddressTwoHanded { get; }
    }
}
