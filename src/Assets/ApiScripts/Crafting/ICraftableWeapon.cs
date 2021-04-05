namespace Assets.ApiScripts.Crafting
{
    public interface ICraftableWeapon : ICraftable
    {
        public enum WeaponCategory
        {
            Melee,
            Ranged,
            Defensive
        }

        /// <summary>
        /// The style of weapon
        /// </summary>
        WeaponCategory SubCategory { get; }

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
