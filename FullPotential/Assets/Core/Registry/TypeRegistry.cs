using FullPotential.Api.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry
{
    public class TypeRegistry : ITypeRegistry
    {
        private readonly List<IGearAccessory> _accessories = new List<IGearAccessory>();
        private readonly List<IGearArmor> _armor = new List<IGearArmor>();
        private readonly List<IGearWeapon> _weapons = new List<IGearWeapon>();
        private readonly List<ILoot> _loot = new List<ILoot>();
        private readonly List<IEffect> _effects = new List<IEffect>();
        private readonly List<ITargeting> _targeting = new List<ITargeting>();
        private readonly List<ITargetingVisuals> _targetingVisuals = new List<ITargetingVisuals>();
        private readonly List<IShape> _shapes = new List<IShape>();
        private readonly List<IShapeVisuals> _shapeVisuals = new List<IShapeVisuals>();
        private readonly Dictionary<string, GameObject> _loadedAddressables = new Dictionary<string, GameObject>();

        public void FindAndRegisterAll(List<string> modPrefixes)
        {
            RegisterCoreTypes();

            foreach (var modPrefix in modPrefixes)
            {
                var asyncOp = Addressables.LoadAssetAsync<GameObject>($"{modPrefix}/Registration");
                asyncOp.Completed += opHandle =>
                {
                    if (opHandle.Result == null)
                    {
                        Debug.LogWarning($"Failed to find registration GameObject for Mod '{modPrefix}'");
                        return;
                    }

                    var mod = opHandle.Result.GetComponent<IMod>();

                    if (mod == null)
                    {
                        Debug.LogWarning($"Failed to find IMod implementation for Mod '{modPrefix}'");
                        return;
                    }

                    HandleModRegistration(mod);
                };
            }
        }

        private void RegisterCoreTypes()
        {
            ValidateAndRegister(typeof(Api.Gameplay.Targeting.PointToPoint));
            ValidateAndRegister(typeof(Api.Gameplay.Targeting.Projectile));
            ValidateAndRegister(typeof(Api.Gameplay.Targeting.Self));
            ValidateAndRegister(typeof(Api.Gameplay.Targeting.Touch));

            ValidateAndRegister(typeof(Api.Gameplay.Shapes.Wall));
            ValidateAndRegister(typeof(Api.Gameplay.Shapes.Zone));
        }

        private void HandleModRegistration(IMod mod)
        {
            foreach (var t in mod.GetRegisterableTypes())
            {
                ValidateAndRegister(t);
            }

            ValidateCrossTypeLinks();

            foreach (var p in mod.GetNetworkPrefabs())
            {
                NetworkManager.Singleton.AddNetworkPrefab(p);
            }
        }

        private void ValidateAndRegister(Type type)
        {
            try
            {
                if (!typeof(IRegisterable).IsAssignableFrom(type))
                {
                    Debug.LogError($"{type.Name} does not implement {nameof(IRegisterable)}");
                    return;
                }

                var toRegister = Activator.CreateInstance(type);

                if (toRegister is IGearAccessory accessory)
                {
                    AddToRegister(_accessories, accessory);
                    return;
                }

                if (toRegister is IGearArmor armor)
                {
                    AddToRegister(_armor, armor);
                    return;
                }

                if (toRegister is IGearWeapon craftableWeapon)
                {
                    AddToRegister(_weapons, craftableWeapon);
                    return;
                }

                if (toRegister is ILoot loot)
                {
                    AddToRegister(_loot, loot);
                    return;
                }

                if (toRegister is IEffect effect)
                {
                    AddToRegister(_effects, effect);
                    return;
                }

                if (toRegister is ITargeting targeting)
                {
                    AddToRegister(_targeting, targeting);
                    return;
                }

                if (toRegister is ITargetingVisuals targetingVisuals)
                {
                    AddToRegister(_targetingVisuals, targetingVisuals);
                    return;
                }

                if (toRegister is IShape shape)
                {
                    AddToRegister(_shapes, shape);
                    return;
                }

                if (toRegister is IShapeVisuals shapeVisuals)
                {
                    AddToRegister(_shapeVisuals, shapeVisuals);
                    return;
                }

                Debug.LogError($"{type.FullName} does not implement any of the valid interfaces");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{type.FullName} failed to register: " + ex);
            }
        }

        private void AddToRegister<T>(List<T> list, T item) where T : IRegisterable
        {
            var match = list.FirstOrDefault(x => x.TypeId == item.TypeId);
            if (match != null)
            {
                Debug.LogError($"A type with ID '{item.TypeId}' has already been registered");
                return;
            }

            list.Add(item);
        }

        private void ValidateCrossTypeLinks()
        {
            var invalidTargetingVisuals = new List<ITargetingVisuals>();
            foreach (var targetingVisual in _targetingVisuals)
            {
                if (_targeting.FirstOrDefault(t => t.TypeId == targetingVisual.TargetingTypeId) == null)
                {
                    Debug.LogError($"{targetingVisual.GetType().FullName} refers to a targeting type that is not registered");
                    invalidTargetingVisuals.Add(targetingVisual);
                }
            }

            foreach (var targetingVisual in invalidTargetingVisuals)
            {
                _targetingVisuals.Remove(targetingVisual);
            }

            var invalidShapeVisuals = new List<IShapeVisuals>();
            foreach (var shapeVisual in _shapeVisuals)
            {
                if (_shapes.FirstOrDefault(t => t.TypeId == shapeVisual.ShapeTypeId) == null)
                {
                    Debug.LogError($"{shapeVisual.GetType().FullName} refers to a shape type that is not registered");
                    invalidShapeVisuals.Add(shapeVisual);
                }
            }
            
            foreach (var shapeVisual in invalidShapeVisuals)
            {
                _shapeVisuals.Remove(shapeVisual);
            }
        }

        public IEnumerable<T> GetRegisteredTypes<T>() where T : IRegisterable
        {
            var interfaceName = typeof(T).Name;
            switch (interfaceName)
            {
                case nameof(IGearAccessory): return (IEnumerable<T>)_accessories;
                case nameof(IGearArmor): return (IEnumerable<T>)_armor;
                case nameof(IGearWeapon): return (IEnumerable<T>)_weapons;
                case nameof(ILoot): return (IEnumerable<T>)_loot;
                case nameof(IEffect): return (IEnumerable<T>)_effects;
                case nameof(IShape): return (IEnumerable<T>)_shapes;
                case nameof(IShapeVisuals): return (IEnumerable<T>)_shapeVisuals;
                case nameof(ITargeting): return (IEnumerable<T>)_targeting;
                case nameof(ITargetingVisuals): return (IEnumerable<T>)_targetingVisuals;
                default: throw new Exception($"Unexpected type {interfaceName}");
            }
        }

        public T GetRegisteredByTypeName<T>(string typeName) where T : IRegisterable
        {
            return GetRegisteredTypes<T>().FirstOrDefault(x => x.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }

        private T GetRegisteredById<T>(string typeId) where T : IRegisterable
        {
            var craftablesOfType = GetRegisteredTypes<T>();

            if (string.IsNullOrWhiteSpace(typeId))
            {
                return (T)(object)null;
            }

            var matches = craftablesOfType.Where(x => x.TypeId == new Guid(typeId)).ToList();

            if (!matches.Any())
            {
                throw new Exception($"Could not find a match for '{typeof(T).Name}' and '{typeId}'");
            }

            if (matches.Count > 1)
            {
                throw new Exception($"How is there more than one match for '{typeof(T).Name}' and '{typeId}'");
            }

            return matches.First();
        }

        public IRegisterable GetRegisteredForItem(ItemBase item)
        {
            switch (item)
            {
                case Accessory:
                    return GetRegisteredById<IGearAccessory>(item.RegistryTypeId);
                case Armor:
                    return GetRegisteredById<IGearArmor>(item.RegistryTypeId);
                case Weapon:
                    return GetRegisteredById<IGearWeapon>(item.RegistryTypeId);
                case Loot:
                    return GetRegisteredById<ILoot>(item.RegistryTypeId);
                default:
                    return null;
            }
        }

        public List<IEffect> GetLootPossibilities()
        {
            return _effects
                .Where(x => x is not IIsSideEffect)
                .ToList();
        }

        public IEffect GetEffect(Guid typeId)
        {
            return _effects.FirstOrDefault(x => x.TypeId == typeId);
        }

        public IEffect GetEffect(Type type)
        {
            return _effects.FirstOrDefault(x => x.GetType() == type);
        }

        public void LoadAddessable(string address, Action<GameObject> action)
        {
            //Addressables.ReleaseInstance(go) : Destroys objects created by Addressables.InstantiateAsync(address)
            //Addressables.Release(opHandle) : Remove the addressable from memory

            if (_loadedAddressables.ContainsKey(address))
            {
                action(_loadedAddressables[address]);
            }
            else
            {
                var asyncOp = Addressables.LoadAssetAsync<GameObject>(address);
                asyncOp.Completed += opHandle =>
                {
                    var prefab = opHandle.Result;

                    if (!_loadedAddressables.ContainsKey(address))
                    {
                        _loadedAddressables.Add(address, prefab);
                    }

                    action(prefab);
                };
            }
        }

    }
}
