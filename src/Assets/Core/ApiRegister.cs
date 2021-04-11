using Assets.ApiScripts.Crafting;
using Assets.Core.Crafting.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Core.Crafting
{
    public class ApiRegister
    {
        private List<IGearAccessory> _accessories = new List<IGearAccessory>();
        private List<IGearArmor> _armor = new List<IGearArmor>();
        private List<IGearWeapon> _weapons = new List<IGearWeapon>();
        private List<IGearLoot> _loot = new List<IGearLoot>();
        private List<IEffect> _effects = new List<IEffect>();

        protected ApiRegister() { }

        private static ApiRegister _instance;
        public static ApiRegister Instance
        {
            get
            {
                return _instance ?? (_instance = new ApiRegister());
            }
        }

        public void FindAndRegisterAll()
        {
            //todo: how do we scan for registrable types?

            foreach (var t in new Standard.Registration().GetRegisterables())
            {
                ValidateAndRegister(t);
            }
        }

        private void ValidateAndRegister(Type type)
        {
            if (!typeof(IRegisterable).IsAssignableFrom(type))
            {
                UnityEngine.Debug.LogError($"{type.Name} does not implement {nameof(IRegisterable)}");
                return;
            }

            var toRegister = (IRegisterable)Activator.CreateInstance(type);

            if (toRegister.TypeId == null)
            {
                UnityEngine.Debug.LogError($"No {nameof(IRegisterable.TypeId)} was specified for class '{type.FullName}'");
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
            else if (toRegister is IEffect effect)
            {
                Register(_effects, effect);
                return;
            }

            UnityEngine.Debug.LogError($"{type.Name} does not implement any of the valid interfaces");

        }

        private void Register<T>(List<T> list, T item) where T : IRegisterable
        {
            var match = list.FirstOrDefault(x => x.TypeId == item.TypeId);
            if (match != null)
            {
                UnityEngine.Debug.LogError($"A type with name '{item.TypeId}' has already been registered");
                return;
            }

            list.Add(item);
        }

        public T GetCraftableTypeByName<T>(string typeName) where T : ICraftable
        {
            var interfaceName = typeof(T).Name;
            switch (interfaceName)
            {
                case nameof(IGearAccessory): return (T)_accessories.FirstOrDefault(x => x.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                case nameof(IGearArmor): return (T)_armor.FirstOrDefault(x => x.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                case nameof(IGearWeapon): return (T)_weapons.FirstOrDefault(x => x.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                case nameof(IGearLoot): return (T)_loot.FirstOrDefault(x => x.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                default: throw new Exception($"Unexpected type {interfaceName}");
            }
        }

        public T GetCraftableType<T>(string typeId) where T : ICraftable
        {
            var craftablesOfType = GetCraftables<T>();

            if (string.IsNullOrWhiteSpace(typeId))
            {
                return (T)(object)null;
            }

            var matches = craftablesOfType.Where(x => x.TypeId == new Guid(typeId));

            if (!matches.Any())
            {
                throw new Exception($"Could not find a match for '{typeof(T).Name}' and '{typeId}'");
            }
            else if (matches.Count() > 1)
            {
                throw new Exception($"How is there more than one match for '{typeof(T).Name}' and '{typeId}'");
            }

            return (T)matches.First();
        }

        public ICraftable GetCraftableType(CraftableBase craftable)
        {
            if (craftable is Accessory)
            {
                return GetCraftableType<IGearAccessory>(craftable.TypeId);
            }
            else if (craftable is Armor)
            {
                return GetCraftableType<IGearArmor>(craftable.TypeId);
            }
            else if (craftable is Weapon)
            {
                return GetCraftableType<IGearWeapon>(craftable.TypeId);
            }
            else if (craftable is Loot)
            {
                return GetCraftableType<IGearLoot>(craftable.TypeId);
            }

            return null;
        }

        public IEnumerable<ICraftable> GetCraftables<T>()
        {
            var interfaceName = typeof(T).Name;
            switch (interfaceName)
            {
                case nameof(IGearAccessory): return _accessories;
                case nameof(IGearArmor): return _armor;
                case nameof(IGearWeapon): return _weapons;
                case nameof(IGearLoot): return _loot;
                default: throw new Exception($"Unexpected type {interfaceName}");
            }
        }

        //todo: un-hardcode this
        public static UnityEngine.GameObject GetPrefabForWeaponType(string typeName, bool twoHanded)
        {
            switch (typeName)
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
                default: throw new Exception($"Unexpected weapon type {typeName}");
            }
        }

        public List<IEffect> GetLootPossibilities()
        {
            return _effects
                .Where(x => !x.IsSideEffect)
                .ToList();
        }

        public IEffect GetEffect(Guid typeId)
        {
            return _effects.FirstOrDefault(x => x.TypeId == typeId);
        }

    }
}
