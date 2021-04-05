using Assets.ApiScripts.Crafting;
using Assets.Standard.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Core.Crafting
{
    public class CraftingRegister
    {
        private List<ICraftable> _registeredCraftables;

        protected CraftingRegister() { }

        private static CraftingRegister _instance;
        public static CraftingRegister Instance
        {
            get
            {
                return _instance ?? (_instance = new CraftingRegister());
            }
        }

        public void FindAndRegisterAll()
        {
            _registeredCraftables = new List<ICraftable>();

            RegisterStandardCraftables();

            //todo: how do we scan for registrable types?
        }

        private void RegisterStandardCraftables()
        {
            _registeredCraftables.Add(new Accessory() { Category = ICraftable.CraftingCategory.Accessory, TypeName = Accessory.Amulet });
            _registeredCraftables.Add(new Accessory() { Category = ICraftable.CraftingCategory.Accessory, TypeName = Accessory.Belt });
            _registeredCraftables.Add(new Accessory() { Category = ICraftable.CraftingCategory.Accessory, TypeName = Accessory.Gloves });
            _registeredCraftables.Add(new Accessory() { Category = ICraftable.CraftingCategory.Accessory, TypeName = Accessory.Ring });

            _registeredCraftables.Add(new Armor() { Category = ICraftable.CraftingCategory.Armor, SubCategory = ICraftableArmor.ArmorCategory.Helm, TypeName = Armor.Helm });
            _registeredCraftables.Add(new Armor() { Category = ICraftable.CraftingCategory.Armor, SubCategory = ICraftableArmor.ArmorCategory.Chest, TypeName = Armor.Chest });
            _registeredCraftables.Add(new Armor() { Category = ICraftable.CraftingCategory.Armor, SubCategory = ICraftableArmor.ArmorCategory.Legs, TypeName = Armor.Legs });
            _registeredCraftables.Add(new Armor() { Category = ICraftable.CraftingCategory.Armor, SubCategory = ICraftableArmor.ArmorCategory.Feet, TypeName = Armor.Feet });
            _registeredCraftables.Add(new Armor() { Category = ICraftable.CraftingCategory.Armor, SubCategory = ICraftableArmor.ArmorCategory.Barrier, TypeName = Armor.Barrier });

            //todo: don't register spells. Register effects
            _registeredCraftables.Add(new Spell() { Category = ICraftable.CraftingCategory.Spell, TypeName = nameof(ICraftable.CraftingCategory.Spell) });

            ValidateAndRegister<Axe>();
            ValidateAndRegister<Bow>();
            ValidateAndRegister<Crossbow>();
            ValidateAndRegister<Dagger>();
            ValidateAndRegister<Gun>();
            ValidateAndRegister<Hammer>();
            ValidateAndRegister<Shield>();
            ValidateAndRegister<Staff>();
            ValidateAndRegister<Sword>();
        }

        private void ValidateAndRegister<T>() where T : new()
        {
            var tType = typeof(T);

            if (!typeof(ICraftable).IsAssignableFrom(tType))
            {
                UnityEngine.Debug.LogError($"{tType.Name} does not implement ICraftable");
                return;
            }

            var toRegister = (ICraftable)new T();

            if (string.IsNullOrWhiteSpace(toRegister.TypeName))
            {
                UnityEngine.Debug.LogError($"No TypeName was specified for class '{tType.FullName}'");
            }

            var match = _registeredCraftables.FirstOrDefault(x => x.TypeName.Equals(toRegister.TypeName, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                UnityEngine.Debug.LogError($"A type with name '{toRegister.TypeName}' has already been registered");
            }

            if (toRegister is ICraftableWeapon craftableWeapon)
            {
                RegisterWeapon(craftableWeapon);
                return;
            }

            UnityEngine.Debug.LogError($"{tType.Name} does not implement any of the valid interfaces");
        }

        private void RegisterWeapon(ICraftableWeapon craftableWeapon)
        {
            //todo: Needed for serialzation but can we keep the oginal class to retain its code for events?

            var defType = typeof(ICraftableWeapon);
            var defProps = defType.GetInterfaces().SelectMany(x => x.GetProperties()).Union(defType.GetProperties());

            var inProps = typeof(Weapon).GetProperties();

            var weapon = new Weapon();

            foreach (var defProp in defProps)
            {
                var inProp = inProps.First(x => x.Name == defProp.Name);
                inProp.SetValue(weapon, defProp.GetValue(craftableWeapon));
            }

            _registeredCraftables.Add(weapon);
        }

        public ICraftable GetCraftingItem(string categoryName, string typeName)
        {
            if (!Enum.TryParse<ICraftableWeapon.CraftingCategory>(categoryName, out var craftableCategory))
            {
                throw new Exception($"Unexpected category '{categoryName}'");
            }

            var matches = _registeredCraftables.Where(x =>
                x.Category == craftableCategory
                && (string.IsNullOrWhiteSpace(typeName) || x.TypeName == typeName)
            );

            if (!matches.Any())
            {
                throw new Exception($"Could not find a match for '{categoryName}' and '{typeName}'");
            }
            else if (matches.Count() > 1)
            {
                throw new Exception($"How is there more than one match for '{categoryName}' and '{typeName}'");
            }

            return matches.First();
        }

        public IEnumerable<ICraftable> GetWeaponCraftables()
        {
            return _registeredCraftables
                .Where(x => x.Category == ICraftable.CraftingCategory.Weapon);
        }

        //todo: un-hardcode this
        public static UnityEngine.GameObject GetPrefabForWeaponType(string weaponType, bool twoHanded)
        {
            switch (weaponType)
            {
                case "Axe": return twoHanded ? GameManager.Instance.Prefabs.Weapons.Axe2 : GameManager.Instance.Prefabs.Weapons.Axe1;
                case "Bow": return GameManager.Instance.Prefabs.Weapons.Bow;
                case "Crossbow": return GameManager.Instance.Prefabs.Weapons.Crossbow;
                case "Dagger": return GameManager.Instance.Prefabs.Weapons.Dagger;
                case "Gun": return twoHanded ? GameManager.Instance.Prefabs.Weapons.Gun2 : GameManager.Instance.Prefabs.Weapons.Gun1;
                case "Hammer": return twoHanded ? GameManager.Instance.Prefabs.Weapons.Hammer2 : GameManager.Instance.Prefabs.Weapons.Hammer1;
                case "Shield": return GameManager.Instance.Prefabs.Weapons.Shield;
                case "Staff": return GameManager.Instance.Prefabs.Weapons.Staff;
                case "Sword": return twoHanded ? GameManager.Instance.Prefabs.Weapons.Sword2 : GameManager.Instance.Prefabs.Weapons.Sword1;
                default: throw new Exception($"Unexpected weapon type {weaponType}");
            }
        }

    }
}
