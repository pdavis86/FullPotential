using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Logging;
using FullPotential.Api.Modding;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Assets.Api.GameManagement;
using FullPotential.Core.Gameplay.Events;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry
{
    public class TypeRegistry : ITypeRegistry
    {
        private readonly IAuditor _logger;
        private readonly IEventBus _eventBus;
        private readonly HashSet<string> _registeredTypeIds = new HashSet<string>();
        private readonly Dictionary<Type, IList> _registeredTypeLists = new Dictionary<Type, IList>();
        private readonly Dictionary<string, object> _loadedAddressables = new Dictionary<string, object>();
        private readonly List<Type> _gameplayTypes;
        private readonly List<Type> _visualsTypes;

        public TypeRegistry(IAuditorFactory auditorFactory, IEventBus eventBus)
        {
            _logger = auditorFactory.Create(this);
            _eventBus = eventBus;

            _gameplayTypes = new List<Type>
            {
                typeof(IResourceType),
                typeof(IAccessoryType),
                typeof(IAmmunitionType),
                typeof(IArmorType),
                typeof(IEffectType),
                typeof(ILootType),
                typeof(IShapeType),
                typeof(ISpecialGearType),
                typeof(ITargetingType),
                typeof(IWeaponType),
                typeof(ISlotType),
                typeof(IElementType),
            };

            _visualsTypes = new List<Type>
            {
                typeof(IAccessoryVisuals),
                typeof(IArmorVisuals),
                typeof(IShapeVisuals),
                typeof(ITargetingVisuals),
                typeof(IWeaponVisuals),
                typeof(ISpecialGearVisuals)
            };
        }

        public void FindAndRegisterAll(List<string> modPrefixes)
        {
            RegisterTypesForAssembly(typeof(IGameManager).Assembly);
            RegisterTypesForAssembly(typeof(TypeRegistry).Assembly);

            foreach (var modPrefix in modPrefixes)
            {
                var asyncOp = Addressables.LoadAssetAsync<GameObject>($"{modPrefix}/Registration");
                asyncOp.Completed += opHandle =>
                {
                    if (opHandle.Result == null)
                    {
                        _logger.Warn("Failed to find registration GameObject for Mod '{0}'", modPrefix);
                        return;
                    }

                    if (!opHandle.Result.TryGetComponent<IMod>(out var mod))
                    {
                        _logger.Warn("Failed to find IMod implementation for Mod '{0}'", modPrefix);
                        return;
                    }

                    RegisterTypesForAssembly(mod.GetType().Assembly);
                    HandleModRegistration(mod);
                };
            }
        }

        private void RegisterTypesForAssembly(Assembly assembly)
        {
            var assemblyTypes = assembly.GetTypes();
            RegisterServices(assemblyTypes);
            RegisterGameplayTypes(assemblyTypes);
            RegisterEventTypes(assemblyTypes);
            RegisterEventHandlerTypes(assemblyTypes);
        }

        private void HandleModRegistration(IMod mod)
        {
            foreach (var address in mod.GetNetworkPrefabAddresses())
            {
                LoadAddessable<GameObject>(address, gameObject =>
                {
                    if (!gameObject.TryGetComponent<NetworkObject>(out var networkObject))
                    {
                        _logger.Error("Cannot register {0} as a Network Prefab as it does not have a NetworkObject component", address);
                        return;
                    }

                    // todo: zzz v0.8 - Is this work-around still needed?
                    //Work-around for https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1499
                    var hashFiledInfo = typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.NonPublic | BindingFlags.Instance);
                    hashFiledInfo.SetValue(networkObject, GenerateHash(address));

                    NetworkManager.Singleton.AddNetworkPrefab(gameObject);
                });
            }
        }

        private static uint GenerateHash(string input)
        {
            using var hasher = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = hasher.ComputeHash(inputBytes);
            return BitConverter.ToUInt32(hashBytes, 0);
        }

        private void ValidateAndRegisterGameplayType(Type type)
        {
            try
            {
                if (!typeof(IRegisterableType).IsAssignableFrom(type))
                {
                    _logger.Error("{0} does not implement {1}", type.Name, nameof(IRegisterableType));
                    return;
                }

                var objectToRegister = DependenciesContext.Dependencies.CreateInstance(type);

                if (!AddToRegisterForInterface(objectToRegister, _gameplayTypes))
                {
                    _logger.Error("{0} does not implement any of the valid interfaces", type.FullName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("{0} failed to register: {1}", type.FullName, ex);
            }
        }

        private void ValidateAndRegisterVisualsType(Type type)
        {
            try
            {
                if (!typeof(IItemVisuals).IsAssignableFrom(type))
                {
                    _logger.Error("{0} does not implement {1}", type.Name, nameof(IItemVisuals));
                    return;
                }

                var objectToRegister = DependenciesContext.Dependencies.CreateInstance(type);
                var objectAsVisuals = (IItemVisuals)objectToRegister;

                if (!_registeredTypeIds.Contains(objectAsVisuals.ApplicableToTypeIdString))
                {
                    _logger.Error("{0} refers to a type that is not registered with ID {1}", objectAsVisuals.GetType().FullName, objectAsVisuals.ApplicableToTypeIdString);
                    return;
                }

                if (!AddToRegisterForInterface(objectToRegister, _visualsTypes))
                {
                    _logger.Error("{0} does not implement any of the valid {1} interfaces", type.FullName, nameof(IItemVisuals));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("{0} failed to register: {1}", type.FullName, ex);
            }
        }

        private bool AddToRegisterForInterface(object objectToRegister, List<Type> typeFilter)
        {
            var interfaces = objectToRegister.GetType().GetInterfaces();
            var registerType = interfaces
                .Where(i => typeFilter.Contains(i))
                .OrderBy(i => interfaces.Any(other => other != i && i.IsAssignableFrom(other)))
                .FirstOrDefault();

            if (registerType == null)
            {
                return false;
            }

            if (objectToRegister is not IRegisterableType objectAsRegisterable
                || !registerType.IsInstanceOfType(objectToRegister))
            {
                return false;
            }

            if (!_registeredTypeLists.ContainsKey(registerType))
            {
                var listType = typeof(List<>).MakeGenericType(registerType);
                _registeredTypeLists.Add(registerType, (IList)Activator.CreateInstance(listType));
            }

            var list = _registeredTypeLists[registerType];

            var match = list.Cast<IRegisterableType>().FirstOrDefault(x => x.TypeId == objectAsRegisterable.TypeId);
            if (match != null)
            {
                _logger.Error("A type with ID '{0}' has already been registered", objectAsRegisterable.TypeId);
                return true;
            }

            _registeredTypeIds.Add(objectAsRegisterable.TypeId.ToString());
            list.Add(objectToRegister);

            return true;
        }

        public IEnumerable<T> GetRegisteredTypes<T>() where T : IRegisterableType
        {
            if (!_registeredTypeLists.ContainsKey(typeof(T)))
            {
                throw new Exception($"Unexpected type '{typeof(T).Name}'");
            }

            return _registeredTypeLists[typeof(T)].Cast<T>();
        }

        public T GetRegisteredByTypeId<T>(string typeIdString) where T : IRegisterableType
        {
            return GetRegisteredTypes<T>().FirstOrDefault(x => x.TypeId.ToString() == typeIdString);
        }

        public IRegisterableType GetAnyRegisteredBySlotId(string typeIdString)
        {
            var typeId = typeIdString.Split(";")[0];

            foreach (var kvp in _registeredTypeLists)
            {
                var match = kvp.Value
                    .Cast<IRegisterableType>()
                    .FirstOrDefault(x => x.TypeId.ToString() == typeId);

                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private T GetRegistryTypeById<T>(string typeId) where T : IRegisterableType
        {
            var craftablesOfType = GetRegisteredTypes<T>();

            if (string.IsNullOrWhiteSpace(typeId))
            {
                return (T)(object)null;
            }

            var matches = craftablesOfType.Where(x => x.TypeId.ToString() == typeId).ToList();
            if (matches.Count == 0)
            {
                throw new Exception($"Could not find a match for '{typeof(T).Name}' and '{typeId}'");
            }

            if (matches.Count > 1)
            {
                throw new Exception($"How is there more than one match for '{typeof(T).Name}' and '{typeId}'");
            }

            return matches[0];
        }

        private IRegisterableType GetItemStackRegistryType(ItemBase item)
        {
            return _registeredTypeLists[typeof(IAmmunitionType)].Cast<IAmmunitionType>().FirstOrDefault(x => x.TypeId.ToString() == item.RegistryTypeId);
        }

        public IRegisterableType GetRegistryTypeForItem(ItemBase item)
        {
            return item switch
            {
                Api.Obsolete.Items.Types.Accessory => GetRegistryTypeById<IAccessoryType>(item.RegistryTypeId),
                Api.Obsolete.Items.Types.Armor => GetRegistryTypeById<IArmorType>(item.RegistryTypeId),
                Api.Obsolete.Items.Types.Weapon => GetRegistryTypeById<IWeaponType>(item.RegistryTypeId),
                Api.Obsolete.Items.Types.Loot => GetRegistryTypeById<ILootType>(item.RegistryTypeId),
                ItemStackBase => GetItemStackRegistryType(item),
                Api.Obsolete.Items.Types.SpecialGear => GetRegistryTypeById<ISpecialGearType>(item.RegistryTypeId),
                _ => null,
            };
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

        private void RegisterServices(Type[] assemblyTypes)
        {
            var services = assemblyTypes
                .Where(type => type.IsClass && !type.IsAbstract)
                .SelectMany(serviceClass => serviceClass.GetInterfaces()
                    .Where(serviceInterface => serviceInterface != typeof(IService)
                        && typeof(IService).IsAssignableFrom(serviceInterface))
                    .Select(serviceInterface => new { serviceInterface, serviceClass }));

            foreach (var service in services)
            {
                DependenciesContext.Dependencies.Register(new Dependency
                {
                    Type = service.serviceInterface,
                    Factory = () => DependenciesContext.Dependencies.CreateInstance(service.serviceClass),
                    IsSingleton = true
                });
            }
        }

        private void RegisterGameplayTypes(Type[] assemblyTypes)
        {
            var classOrStructTypes = assemblyTypes
                .Where(type => (type.IsClass || (type.IsValueType && !type.IsEnum))
                    && !type.IsAbstract
                    && typeof(IRegisterableType).IsAssignableFrom(type))
                .Except(_gameplayTypes)
                .Except(_visualsTypes);

            var registerableTypes = classOrStructTypes
                .Where(type => !typeof(IItemVisuals).IsAssignableFrom(type));
            foreach (var type in registerableTypes)
            {
                ValidateAndRegisterGameplayType(type);
            }

            var visualsTypes = classOrStructTypes
                .Where(type => typeof(IItemVisuals).IsAssignableFrom(type));
            foreach (var type in visualsTypes)
            {
                ValidateAndRegisterVisualsType(type);
            }
        }

        private void RegisterEventTypes(Type[] assemblyTypes)
        {
            var eventTypes = assemblyTypes
                .Where(t => t != typeof(IEvent) && typeof(IEvent).IsAssignableFrom(t))
                .ToList();

            foreach (var eventType in eventTypes)
            {
                try
                {
                    _eventBus.Register(eventType);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        private void RegisterEventHandlerTypes(Type[] assemblyTypes)
        {
            var eventHandlerTypes = assemblyTypes
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
                .ToList();

            foreach (var handlerType in eventHandlerTypes)
            {
                if (handlerType.IsGenericType && handlerType.GetGenericTypeDefinition() == typeof(BasicEventHandler<>))
                {
                    continue;
                }

                try
                {
                    _eventBus.Subscribe(handlerType);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }
    }
}
