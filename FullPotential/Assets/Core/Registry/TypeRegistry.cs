using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FullPotential.Api.Gameplay.Events;
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
        private readonly Dictionary<Type, IList> _registeredTypeLists = new Dictionary<Type, IList>();
        private readonly List<IVisuals> _visualsToCheck = new List<IVisuals>();
        private readonly Dictionary<string, GameObject> _loadedAddressables = new Dictionary<string, GameObject>();
        private readonly IEventManager _eventManager;
        private readonly Func<object, bool>[] _functionsToRun;

        public TypeRegistry(IEventManager eventManager)
        {
            _eventManager = eventManager;

            _functionsToRun = new Func<object, bool>[]
            {
                AddToRegister<IAccessory>,
                AddToRegister<IAccessoryVisuals>,
                AddToRegister<IAmmunition>,
                AddToRegister<IArmor>,
                AddToRegister<IArmorVisuals>,
                AddToRegister<IEffect>,
                AddToRegister<ILoot>,
                AddToRegister<IShape>,
                AddToRegister<IShapeVisuals>,
                AddToRegister<ITargeting>,
                AddToRegister<ITargetingVisuals>,
                AddToRegister<IWeapon>,
                AddToRegister<IWeaponVisuals>
            };
        }

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

            ValidateIVisuals();

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

            mod.RegisterEventHandlers(_eventManager);
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

                var objectToRegister = Activator.CreateInstance(type);

                foreach (var functionToRun in _functionsToRun)
                {
                    if (functionToRun(objectToRegister))
                    {
                        return;
                    }
                }

                Debug.LogError($"{type.FullName} does not implement any of the valid interfaces");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{type.FullName} failed to register: " + ex);
            }
        }

        private bool AddToRegister<T>(object objectToRegister) where T : IRegisterable
        {
            if (objectToRegister is not T objectAsT)
            {
                return false;
            }

            if (!_registeredTypeLists.ContainsKey(typeof(T)))
            {
                _registeredTypeLists.Add(typeof(T), new List<T>());
            }

            var list = _registeredTypeLists[typeof(T)];

            var match = list.Cast<T>().FirstOrDefault(x => x.TypeId == objectAsT.TypeId);
            if (match != null)
            {
                Debug.LogError($"A type with ID '{objectAsT.TypeId}' has already been registered");
                return true;
            }

            list.Add(objectAsT);

            if (objectToRegister is IVisuals visuals)
            {
                _visualsToCheck.Add(visuals);
            }

            return true;
        }

        private void ValidateIVisuals()
        {
            //todo: is this necessary?
            //var invalidVisuals = new List<IVisuals>();
            //foreach (var visual in _visualsToCheck)
            //{
            //    if (_targetingTypes.FirstOrDefault(t => t.TypeId == visual.ApplicableToTypeId) == null)
            //    {
            //        Debug.LogError($"{visual.GetType().FullName} refers to a type that is not registered with ID {visual.ApplicableToTypeId}");
            //        invalidVisuals.Add(visual);
            //    }
            //}

            //foreach (var targetingVisual in invalidVisuals)
            //{
            //    _targetingVisuals.Remove(targetingVisual);
            //}
        }

        public IEnumerable<T> GetRegisteredTypes<T>() where T : IRegisterable
        {
            if (!_registeredTypeLists.ContainsKey(typeof(T)))
            {
                throw new Exception($"Unexpected type '{typeof(T).Name}'");
            }

            return _registeredTypeLists[typeof(T)].Cast<T>();
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
                    return GetRegisteredById<IAccessory>(item.RegistryTypeId);
                case Armor:
                    return GetRegisteredById<IArmor>(item.RegistryTypeId);
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
            return _registeredTypeLists[typeof(IEffect)]
                .Cast<IEffect>()
                .Where(x => x is not IIsSideEffect)
                .ToList();
        }

        //todo: are both of these necessary?
        public IEffect GetEffect(Guid typeId)
        {
            return _registeredTypeLists[typeof(IEffect)]
                .Cast<IEffect>()
                .FirstOrDefault(x => x.TypeId == typeId);
        }

        //todo: are both of these necessary?
        public IEffect GetEffect(Type type)
        {
            return _registeredTypeLists[typeof(IEffect)]
                .Cast<IEffect>()
                .FirstOrDefault(x => x.GetType() == type);
        }

        public void LoadAddessable(string address, Action<GameObject> action)
        {
            //Addressables.ReleaseInstance(go) : Destroys objects created by Addressables.InstantiateAsync(address)
            //Addressables.Release(opHandle) : Remove the addressable from memory

            if (_loadedAddressables.TryGetValue(address, out var loadedAddressable))
            {
                action(loadedAddressable);
            }
            else
            {
                var asyncOp = Addressables.LoadAssetAsync<GameObject>(address);
                asyncOp.Completed += opHandle =>
                {
                    var prefab = opHandle.Result;

                    _loadedAddressables.TryAdd(address, prefab);

                    action(prefab);
                };
            }
        }

    }
}
