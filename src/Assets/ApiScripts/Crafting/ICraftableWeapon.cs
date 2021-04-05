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

        WeaponCategory SubCategory { get; }
        bool AllowAutomatic { get; }
        bool AllowTwoHanded { get; }
        bool EnforceTwoHanded { get; }
    }
}
