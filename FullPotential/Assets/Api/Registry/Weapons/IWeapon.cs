namespace FullPotential.Api.Registry.Weapons
{
    public interface IWeapon : IRegisterable
    {
        /// <summary>
        /// This weapon is used primarily for defense
        /// </summary>
        bool IsDefensive { get; }

        /// <summary>
        /// Optional, what type of ammunition is used
        /// </summary>
        string AmmunitionTypeIdString { get; }

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
    }
}
