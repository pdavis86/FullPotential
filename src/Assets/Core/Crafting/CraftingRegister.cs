using Assets.ApiScripts.Crafting;
using Assets.Core.Crafting.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Core.Crafting
{
    public class CraftingRegister
    {
        private List<IGearAccessory> _accessories = new List<IGearAccessory>();
        private List<IGearArmor> _armor = new List<IGearArmor>();
        private List<IGearWeapon> _weapons = new List<IGearWeapon>();
        private List<IGearLoot> _loot = new List<IGearLoot>();

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
            RegisterStandardCraftables();

            //todo: how do we scan for registrable types?
        }

        private void RegisterStandardCraftables()
        {
            ValidateAndRegister<Standard.Accessories.Amulet>();
            ValidateAndRegister<Standard.Accessories.Belt>();
            ValidateAndRegister<Standard.Accessories.Ring>();

            ValidateAndRegister<Standard.Armor.Helm>();
            ValidateAndRegister<Standard.Armor.Chest>();
            ValidateAndRegister<Standard.Armor.Legs>();
            ValidateAndRegister<Standard.Armor.Feet>();
            ValidateAndRegister<Standard.Armor.Barrier>();

            ValidateAndRegister<Standard.Weapons.Axe>();
            ValidateAndRegister<Standard.Weapons.Bow>();
            ValidateAndRegister<Standard.Weapons.Crossbow>();
            ValidateAndRegister<Standard.Weapons.Dagger>();
            ValidateAndRegister<Standard.Weapons.Gun>();
            ValidateAndRegister<Standard.Weapons.Hammer>();
            ValidateAndRegister<Standard.Weapons.Shield>();
            ValidateAndRegister<Standard.Weapons.Staff>();
            ValidateAndRegister<Standard.Weapons.Sword>();

            ValidateAndRegister<Standard.Loot.Scrap>();
            ValidateAndRegister<Standard.Loot.Shard>();
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

            if (toRegister is IGearAccessory accessory)
            {
                Register(_accessories, accessory);
                return;
            }
            else if (toRegister is IGearArmor armor)
            {
                Register(_armor, armor);
                return;
            }
            else if (toRegister is IGearWeapon craftableWeapon)
            {
                Register(_weapons, craftableWeapon);
                return;
            }
            else if (toRegister is IGearLoot loot)
            {
                Register(_loot, loot);
                return;
            }

            UnityEngine.Debug.LogError($"{tType.Name} does not implement any of the valid interfaces");
        }

        private void Register<T>(List<T> list, T item) where T : ICraftable
        {
            var match = list.FirstOrDefault(x => x.TypeName.Equals(item.TypeName, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                UnityEngine.Debug.LogError($"A type with name '{item.TypeName}' has already been registered");
                return;
            }

            list.Add(item);
        }

        public T GetCraftableType<T>(string typeName) where T : ICraftable
        {
            var matches = GetCraftables<T>().Where(x => x.TypeName == typeName);

            if (!matches.Any())
            {
                throw new Exception($"Could not find a match for '{typeof(T).Name}' and '{typeName}'");
            }
            else if (matches.Count() > 1)
            {
                throw new Exception($"How is there more than one match for '{typeof(T).Name}' and '{typeName}'");
            }

            return (T)matches.First();
        }

        public ICraftable GetCraftableType(CraftableBase craftable)
        {
            if (craftable is Accessory)
            {
                return GetCraftableType<IGearAccessory>(craftable.TypeName);
            }
            else if (craftable is Armor)
            {
                return GetCraftableType<IGearArmor>(craftable.TypeName);
            }
            else if (craftable is Weapon)
            {
                return GetCraftableType<IGearWeapon>(craftable.TypeName);
            }
            else if (craftable is Loot)
            {
                return GetCraftableType<IGearLoot>(craftable.TypeName);
            }

            return null;
        }

        public IEnumerable<ICraftable> GetCraftables<T>()
        {
            var typeName = typeof(T).Name;
            switch (typeName)
            {
                case nameof(IGearAccessory): return _accessories;
                case nameof(IGearArmor): return _armor;
                case nameof(IGearWeapon): return _weapons;
                case nameof(IGearLoot): return _loot;
                default: throw new Exception($"Unexpected category {typeName}");
            }
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
