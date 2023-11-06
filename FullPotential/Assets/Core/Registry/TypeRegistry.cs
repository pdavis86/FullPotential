using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry
{
    public class TypeRegistry : ITypeRegistry
    {
        private readonly List<IAccessoryVisuals> _accessoryVisuals = new List<IAccessoryVisuals>();
        private readonly List<IArmorVisuals> _armorVisuals = new List<IArmorVisuals>();
        private readonly List<ITargetingVisuals> _targetingVisuals = new List<ITargetingVisuals>();
        private readonly List<IShapeVisuals> _shapeVisuals = new List<IShapeVisuals>();

        private readonly List<IWeapon> _weaponTypes = new List<IWeapon>();
        private readonly List<ILoot> _lootTypes = new List<ILoot>();
        private readonly List<IAmmunition> _ammoTypes = new List<IAmmunition>();
        private readonly List<IEffect> _effectTypes = new List<IEffect>();
        private readonly List<ITargeting> _targetingTypes = new List<ITargeting>();
        private readonly List<IShape> _shapeTypes = new List<IShape>();

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

            foreach (var address in mod.GetNetworkPrefabAddresses())
            {
                LoadAddessable(address, gameObject =>
                {
                    var networkObject = gameObject.GetComponent<NetworkObject>();

                    if (networkObject == null)
                    {
                        Debug.LogError($"Cannot register {address} as a Network Prefab as it does not have a NetworkObject component");
                        return;
                    }

                    //todo: zzz v0.5 - use gameObject.GetHashCode().ToString() instead of address in GenerateHash() to get the "NetworkConfig mismatch" issue
                    // then you can figure out how to tell the client that just tried to join

                    //Work-around for https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1499
                    var hashFiledInfo = typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.NonPublic | BindingFlags.Instance);
                    hashFiledInfo!.SetValue(networkObject, GenerateHash(address));

                    NetworkManager.Singleton.AddNetworkPrefab(gameObject);
                });
            }
        }

        private static uint GenerateHash(string input)
        {
            using (var hasher = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = hasher.ComputeHash(inputBytes);
                return BitConverter.ToUInt32(hashBytes, 0);
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

                if (toRegister is IAccessoryVisuals accessory)
                {
                    AddToRegister(_accessoryVisuals, accessory);
                    return;
                }

                if (toRegister is IArmorVisuals armor)
                {
                    AddToRegister(_armorVisuals, armor);
                    return;
                }

                if (toRegister is IAmmunition ammoType)
                {
                    AddToRegister(_ammoTypes, ammoType);
                    return;
                }

                if (toRegister is IWeapon craftableWeapon)
                {
                    AddToRegister(_weaponTypes, craftableWeapon);
                    return;
                }

                if (toRegister is ILoot loot)
                {
                    AddToRegister(_lootTypes, loot);
                    return;
                }

                if (toRegister is IEffect effect)
                {
                    AddToRegister(_effectTypes, effect);
                    return;
                }

                if (toRegister is ITargeting targeting)
                {
                    AddToRegister(_targetingTypes, targeting);
                    return;
                }

                if (toRegister is ITargetingVisuals targetingVisuals)
                {
                    AddToRegister(_targetingVisuals, targetingVisuals);
                    return;
                }

                if (toRegister is IShape shape)
                {
                    AddToRegister(_shapeTypes, shape);
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
                if (_targetingTypes.FirstOrDefault(t => t.TypeId == targetingVisual.TargetingTypeId) == null)
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
                if (_shapeTypes.FirstOrDefault(t => t.TypeId == shapeVisual.ShapeTypeId) == null)
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
                case nameof(IAccessoryVisuals): return (IEnumerable<T>)_accessoryVisuals;
                case nameof(IArmorVisuals): return (IEnumerable<T>)_armorVisuals;
                case nameof(IWeapon): return (IEnumerable<T>)_weaponTypes;
                case nameof(ILoot): return (IEnumerable<T>)_lootTypes;
                case nameof(IAmmunition): return (IEnumerable<T>)_ammoTypes;
                case nameof(IEffect): return (IEnumerable<T>)_effectTypes;
                case nameof(IShape): return (IEnumerable<T>)_shapeTypes;
                case nameof(IShapeVisuals): return (IEnumerable<T>)_shapeVisuals;
                case nameof(ITargeting): return (IEnumerable<T>)_targetingTypes;
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
                    return GetRegisteredById<IAccessoryVisuals>(item.RegistryTypeId);
                case Armor:
                    return GetRegisteredById<IArmorVisuals>(item.RegistryTypeId);
                case Weapon:
                    return GetRegisteredById<IWeapon>(item.RegistryTypeId);
                case Loot:
                    return GetRegisteredById<ILoot>(item.RegistryTypeId);
                default:
                    return null;
            }
        }

        public List<IEffect> GetLootPossibilities()
        {
            return _effectTypes
                .Where(x => x is not IIsSideEffect)
                .ToList();
        }

        public IEffect GetEffect(Guid typeId)
        {
            return _effectTypes.FirstOrDefault(x => x.TypeId == typeId);
        }

        public IEffect GetEffect(Type type)
        {
            return _effectTypes.FirstOrDefault(x => x.GetType() == type);
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
