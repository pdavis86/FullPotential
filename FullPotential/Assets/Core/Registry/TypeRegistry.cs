using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Modding;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Core.Registry.Events;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry
{
    public class TypeRegistry : ITypeRegistry
    {
        private readonly HashSet<string> _registeredTypeIds = new HashSet<string>();
        private readonly Dictionary<Type, IList> _registeredTypeLists = new Dictionary<Type, IList>();
        private readonly Dictionary<string, object> _loadedAddressables = new Dictionary<string, object>();
        private readonly IEventManager _eventManager;
        private readonly Func<object, bool>[] _registerTypeFunctions;
        private readonly Func<object, bool>[] _registerVisualsFunctions;

        public TypeRegistry(IEventManager eventManager)
        {
            _eventManager = eventManager;

            _registerTypeFunctions = new Func<object, bool>[]
            {
                AddToRegister<IResource>,
                AddToRegister<IAccessory>,
                AddToRegister<IAmmunition>,
                AddToRegister<IArmor>,
                AddToRegister<IEffect>,
                AddToRegister<ILoot>,
                AddToRegister<IShape>,
                AddToRegister<ISpecialGear>,
                AddToRegister<ITargeting>,
                AddToRegister<IWeapon>,
                AddToRegister<IRegisterableWithSlot>,
                AddToRegister<IEffectComputation>,
            };

            _registerVisualsFunctions = new Func<object, bool>[]
            {
                AddToRegister<IAccessoryVisuals>,
                AddToRegister<IArmorVisuals>,
                AddToRegister<IShapeVisuals>,
                AddToRegister<ITargetingVisuals>,
                AddToRegister<IWeaponVisuals>,
                AddToRegister<ISpecialGearVisuals>
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
            ValidateAndRegister(typeof(SpecialSlots.LeftHand));
            ValidateAndRegister(typeof(SpecialSlots.RightHand));

            ValidateAndRegister(typeof(Resources.Health));
            ValidateAndRegister(typeof(Resources.Stamina));

            ValidateAndRegister(typeof(Effects.Heal));
            ValidateAndRegister(typeof(Effects.Hurt));
            ValidateAndRegister(typeof(Effects.Push));

            ValidateAndRegister(typeof(Combat.HurtEffectComputation));

            _eventManager.Subscribe<LivingEntityDiedEventHandler>(LivingEntityBase.EventIdResourceValueChanged);
            _eventManager.Subscribe<LivingEntityHealthChangedEventHandler>(LivingEntityBase.EventIdResourceValueChanged);
        }

        private void HandleModRegistration(IMod mod)
        {
            foreach (var t in mod.GetRegisterableTypes())
            {
                ValidateAndRegister(t);
            }

            foreach (var t in mod.GetRegisterableVisuals())
            {
                ValidateAndRegisterVisuals(t);
            }

            foreach (var address in mod.GetNetworkPrefabAddresses())
            {
                LoadAddessable<GameObject>(address, gameObject =>
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

                var objectToRegister = DependenciesContext.Dependencies.CreateInstance(type);

                foreach (var functionToRun in _registerTypeFunctions)
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

        private void ValidateAndRegisterVisuals(Type type)
        {
            try
            {
                if (!typeof(IItemVisuals).IsAssignableFrom(type))
                {
                    Debug.LogError($"{type.Name} does not implement {nameof(IItemVisuals)}");
                    return;
                }

                var objectToRegister = DependenciesContext.Dependencies.CreateInstance(type);
                var objectAsVisuals = (IItemVisuals)objectToRegister;

                if (!_registeredTypeIds.Contains(objectAsVisuals.ApplicableToTypeIdString))
                {
                    Debug.LogError($"{objectAsVisuals.GetType().FullName} refers to a type that is not registered with ID {objectAsVisuals.ApplicableToTypeIdString}");
                    return;
                }

                foreach (var functionToRun in _registerVisualsFunctions)
                {
                    if (functionToRun(objectToRegister))
                    {
                        return;
                    }
                }

                Debug.LogError($"{type.FullName} does not implement any of the valid {nameof(IItemVisuals)} interfaces");
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

            _registeredTypeIds.Add(objectAsT.TypeId.ToString());
            list.Add(objectAsT);

            return true;
        }

        public IEnumerable<T> GetRegisteredTypes<T>() where T : IRegisterable
        {
            if (!_registeredTypeLists.ContainsKey(typeof(T)))
            {
                throw new Exception($"Unexpected type '{typeof(T).Name}'");
            }

            return _registeredTypeLists[typeof(T)].Cast<T>();
        }

        public T GetRegisteredByTypeId<T>(string typeId) where T : IRegisterable
        {
            return GetRegisteredTypes<T>().FirstOrDefault(x => x.TypeId.ToString() == typeId);
        }

        public IRegisterable GetAnyRegisteredBySlotId(string slotId)
        {
            var typeId = slotId.Split(";")[0];

            foreach (var kvp in _registeredTypeLists)
            {
                var match = kvp.Value
                    .Cast<IRegisterable>()
                    .FirstOrDefault(x => x.TypeId.ToString() == typeId);

                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private T GetRegistryTypeById<T>(string typeId) where T : IRegisterable
        {
            var craftablesOfType = GetRegisteredTypes<T>();

            if (string.IsNullOrWhiteSpace(typeId))
            {
                return (T)(object)null;
            }

            var matches = craftablesOfType.Where(x => x.TypeId.ToString() == typeId).ToList();

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

        private IRegisterable GetItemStackRegistryType(ItemBase item)
        {
            return _registeredTypeLists[typeof(IAmmunition)].Cast<IAmmunition>().FirstOrDefault(x => x.TypeId.ToString() == item.RegistryTypeId);
        }

        public IRegisterable GetRegistryTypeForItem(ItemBase item)
        {
            switch (item)
            {
                case Api.Items.Types.Accessory:
                    return GetRegistryTypeById<IAccessory>(item.RegistryTypeId);
                case Api.Items.Types.Armor:
                    return GetRegistryTypeById<IArmor>(item.RegistryTypeId);
                case Api.Items.Types.Weapon:
                    return GetRegistryTypeById<IWeapon>(item.RegistryTypeId);
                case Api.Items.Types.Loot:
                    return GetRegistryTypeById<ILoot>(item.RegistryTypeId);
                case Api.Items.Types.ItemStack:
                    return GetItemStackRegistryType(item);
                case Api.Items.Types.SpecialGear:
                    return GetRegistryTypeById<ISpecialGear>(item.RegistryTypeId);
                default:
                    return null;
            }
        }

        public IEffect GetEffect(string typeId)
        {
            return _registeredTypeLists[typeof(IEffect)]
                .Cast<IEffect>()
                .FirstOrDefault(x => x.TypeId.ToString() == typeId);
        }

        public void LoadAddessable<T>(string address, Action<T> action)
        {
            //Addressables.ReleaseInstance(go) : Destroys objects created by Addressables.InstantiateAsync(address)
            //Addressables.Release(opHandle) : Remove the addressable from memory

            if (_loadedAddressables.TryGetValue(address, out var loadedAddressable))
            {
                action((T)loadedAddressable);
            }
            else
            {
                var asyncOp = Addressables.LoadAssetAsync<T>(address);
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
