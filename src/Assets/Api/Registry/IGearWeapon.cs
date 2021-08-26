namespace FullPotential.Assets.Api.Registry
{
    public interface IGearWeapon : IGear
    {
        public enum WeaponCategory
        {
            Melee,
            Ranged,
            Defensive
        }

        /// <summary>
        /// The category of weapon
        /// </summary>
        WeaponCategory Category { get; }

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
