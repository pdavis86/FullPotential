using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Data;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Gameplay.Inventory.Events;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Gameplay.Player.Models;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete.Items.Base;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.Player;

using Unity.Netcode;

using UnityEngine;

// ReSharper disable UnusedMemberHierarchy.Global
// ReSharper disable MemberCanBePrivate.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class InventoryBase : NetworkBehaviour, ISaveable
    {
        public const string SlotChangeEventId = "9c7972de-4136-4825-aaa3-11925ad049ee";

        // todo: remove _itemIdToShapeMapping
        protected readonly Dictionary<string, string> _itemIdToShapeMapping = new Dictionary<string, string>();

        // ReSharper disable InconsistentNaming
        protected IItemFactory _itemFactory;
        protected ITypeRegistry _typeRegistry;
        protected ILocalizer _localizer;
        protected IRpcService _rpcService;
        protected IEventBus _eventBus;
        protected ISaveManager _saveManager;

        protected bool _isDirty;
        protected LivingEntityBase _livingEntity;
        protected string _characterId;
        protected bool _hasInventoryLoaded;
        protected Dictionary<string, ItemBase> _items;
        protected Dictionary<string, EquippedItem> _equippedItems;
        // ReSharper restore InconsistentNaming

        public bool IsDirty => _isDirty || _items.Any(x => x.Value.IsDirty);

        #region Unity Events Handlers

        protected virtual void Awake()
        {
            _itemFactory = DependenciesContext.Dependencies.GetService<IItemFactory>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();
            _eventBus = DependenciesContext.Dependencies.GetService<IEventBus>();
            _saveManager = DependenciesContext.Dependencies.GetService<ISaveManager>();

            _livingEntity = GetComponent<LivingEntityBase>();

            _items = new Dictionary<string, ItemBase>();
            _equippedItems = new Dictionary<string, EquippedItem>();
        }

        #endregion

        // ReSharper disable once UnusedParameter.Global
        [ClientRpc]
        public void ApplyChangesClientRpc(InventoryChangesForClient changes, ClientRpcParams clientRpcParams)
        {
            // todo: ApplyChangesClientRpc
        }

        public void LoadInventory(InventoryData inventoryData)
        {
            _characterId = inventoryData.CharacterId;

            foreach (var item in inventoryData.Items)
            {
                _items.Add(item.Id, _itemFactory.GetItemFromData(item));
            }

            if (inventoryData.EquippedItems != null)
            {
                ApplyEquippedItemChanges(inventoryData.EquippedItems);
            }

            _hasInventoryLoaded = true;
        }

        public bool ApplyInventoryChanges(InventoryData changes)
        {
            var itemsRemoved = new List<ItemBase>();
            foreach (var item in changes.Items.Where(x => x.IsDeleted))
            {
                if (!_items.ContainsKey(item.Id))
                {
                    Debug.LogWarning($"Could not remove item with ID {item.Id}. Was this admin crafting?");
                    continue;
                }

                itemsRemoved.Add(_items[item.Id]);
                _items.Remove(item.Id);
            }

            //todo: zzz v0.6 - should be able to take item stacks when inventory is full if there is space
            //if (IsInventoryFull())
            //{
            //    NotifyOfInventoryFull();
            //    return false;
            //}

            var allItems = changes.Items.Select(x => _itemFactory.GetItemFromData(x));
            var itemStacks = allItems.Where(x => typeof(IItemStack).IsAssignableFrom(x.GetType()));
            var nonItemStacks = allItems.Where(x => !typeof(IItemStack).IsAssignableFrom(x.GetType()));

            var itemsToAdd = new List<ItemBase>();

            foreach (var itemStack in itemStacks.Select(x => (ItemStackBase)x))
            {
                var newStack = MergeItemStacks(itemStack);
                if (newStack != null)
                {
                    itemsToAdd.Add(newStack);
                }
            }

            foreach (var item in nonItemStacks)
            {
                if (_items.ContainsKey(item.Id))
                {
                    _items[item.Id] = item;
                }
                else
                {
                    _items.Add(item.Id, item);
                    itemsToAdd.Add(item);
                }
            }

            // todo: fire an event instead TriggerInventoryChangedEvent
            NotifyOfItemsRemoved(itemsRemoved);
            NotifyOfItemsAdded(itemsToAdd);
            ApplyEquippedItemChanges(changes.EquippedItems);

            return true;
        }

        private T CastItemAsType<T>(ItemBase item, bool errorIfNotFound, string identifierName, string id) where T : ItemBase
        {
            if (item == null)
            {
                if (errorIfNotFound)
                {
                    Debug.LogError($"Could not find the item with {identifierName} '{id}'");
                }
                return null;
            }

            if (item is not T castAsType)
            {
                throw new Exception($"Item '{item.Id}' was not of the correct type: {typeof(T).Name}");
            }

            return castAsType;
        }

        public T GetItemWithId<T>(string id, bool errorIfNotFound = true) where T : ItemBase
        {
            var item = _items.FirstOrDefault(x => x.Value.Id == id).Value;
            return CastItemAsType<T>(item, errorIfNotFound, "ID", id);
        }

        public T GetItemInSlot<T>(string slotId, bool errorIfNotFound = false) where T : ItemBase
        {
            var item = GetItemInSlot(slotId);
            return CastItemAsType<T>(item, errorIfNotFound, "slot ID", slotId);
        }

        public ItemBase GetItemInSlot(string slotId)
        {
            return _equippedItems.TryGetValue(slotId, out var equippedItem)
                ? equippedItem.Item
                : null;
        }

        public int TakeCountFromItemStacks(string typeId, int count)
        {
            var matches = _items
                .Where(x => x.Value.RegistryTypeId == typeId)
                .Select(x => (ItemStackBase)x.Value)
                .OrderBy(x => x.Count);

            if (!matches.Any())
            {
                return 0;
            }

            var countRemaining = count;

            foreach (var itemStack in matches)
            {
                if (countRemaining >= itemStack.Count)
                {
                    countRemaining -= itemStack.Count;
                    _items.Remove(itemStack.Id);
                    continue;
                }

                itemStack.Count -= countRemaining;
                countRemaining = 0;

                if (itemStack.Count == 0)
                {
                    _items.Remove(itemStack.Id);
                }

                break;
            }

            MarkAsDirtyAndAddToQueue();

            return count - countRemaining;
        }

        public int GetItemStackTotal(string typeId)
        {
            return _items
                .Where(
                    i => i.Value is ItemStackBase itemStack
                    && itemStack.RegistryTypeId == typeId)
                .Select(i => (ItemStackBase)i.Value)
                .Sum(i => i.Count);
        }

        public bool IsInventoryFull()
        {
            // todo: return _items.Count >= _maxItemCount;
            return false;
        }

        public List<CombatItemBase> GetComponentsFromIds(string[] componentIds)
        {
            //Check that the components are actually in the player's inventory and load them in the order they are given
            var components = new List<CombatItemBase>();
            foreach (var id in componentIds)
            {
                var match = GetItemWithId<CombatItemBase>(id);
                if (match != null)
                {
                    components.Add(match);
                }
            }
            return components;
        }

        public List<string> ValidateIsCraftable(string[] componentIds, ItemBase itemToCraft)
        {
            if (componentIds == null || componentIds.Length == 0)
            {
                return new List<string> { _localizer.Translate("crafting.error.nocomponents") };
            }

            var components = GetComponentsFromIds(componentIds);

            // todo: move these into type definitions
            var errors = new List<string>();
            if (itemToCraft is Consumer consumerItem)
            {
                if (consumerItem.EffectIds.Length == 0)
                {
                    errors.Add(_localizer.Translate("crafting.error.missingeffect"));
                }
            }
            else if (itemToCraft is Weapon weapon)
            {
                if (components.Count > 8)
                {
                    errors.Add(_localizer.Translate("crafting.error.toomanycomponents"));
                }
                if (components.Count > 4 && !weapon.IsTwoHanded)
                {
                    errors.Add(_localizer.Translate("crafting.error.toomanyforonehanded"));
                }
            }

            return errors;
        }

        protected bool IsValidSlotId(string slotId)
        {
            if (slotId is HandSlotIds.LeftHand or HandSlotIds.RightHand)
            {
                return true;
            }

            return _typeRegistry.GetRegisteredTypes<IArmorType>().FirstOrDefault(t => t.TypeId.ToString() == slotId) != null
                   || _typeRegistry.GetRegisteredTypes<IAccessoryType>().FirstOrDefault(t => slotId.StartsWith(t.TypeId.ToString())) != null
                   || _typeRegistry.GetRegisteredTypes<IRegisterableWithSlotType>().FirstOrDefault(t => t.TypeId.ToString() == slotId) != null;
        }

        private ItemBase MergeItemStacks(ItemStackBase newStack)
        {
            if (!IsServer)
            {
                Debug.LogError("MergeItemStacks called client-side");
                return null;
            }

            var partiallyFullStacks = _items
                .Where(
                    i => i.Value is ItemStackBase oldStack
                    && oldStack.RegistryTypeId == newStack.RegistryTypeId
                    && oldStack.Count < oldStack.MaxSize)
                .Select(i => (ItemStackBase)i.Value);

            if (!partiallyFullStacks.Any())
            {
                if (_items.ContainsKey(newStack.Id))
                {
                    _items[newStack.Id] = newStack;
                    return null;
                }

                _items.Add(newStack.Id, newStack);
                return newStack;
            }

            var itemsRemaining = newStack.Count;

            foreach (var partiallyFullStack in partiallyFullStacks)
            {
                var space = partiallyFullStack.MaxSize - partiallyFullStack.Count;

                if (space <= itemsRemaining)
                {
                    partiallyFullStack.Count = partiallyFullStack.MaxSize;
                    itemsRemaining -= space;
                    continue;
                }

                partiallyFullStack.Count += itemsRemaining;
                itemsRemaining = 0;
                break;
            }

            if (itemsRemaining > 0)
            {
                newStack.Count = itemsRemaining;

                if (_items.ContainsKey(newStack.Id))
                {
                    _items[newStack.Id] = newStack;
                    return null;
                }

                _items.Add(newStack.Id, newStack);
                return newStack;
            }

            return null;
        }

        protected (bool WasEquipped, List<string> SlotsToSend) HandleSlotChange(ItemBase item, string slotId)
        {
            var slotsToSend = new List<string> { slotId };

            var previousKvp = _equippedItems
                .FirstOrDefault(x => x.Value.Item != null && x.Value?.Item.Id == item.Id);

            var previousSlotId = previousKvp.Value != null ? previousKvp.Key : null;

            if (!previousSlotId.IsNullOrWhiteSpace())
            {
                if (previousSlotId != slotId)
                {
                    slotsToSend.Add(previousSlotId);
                }

                _equippedItems[previousSlotId!].Item = null;

                TriggerSlotChangeEvent(null, slotId);
            }

            var wasEquipped = false;
            if (previousSlotId.IsNullOrWhiteSpace() || previousSlotId != slotId)
            {
                TriggerSlotChangeEvent(item.Id, slotId);
                wasEquipped = true;
            }

            if (slotId == HandSlotIds.LeftHand || slotId == HandSlotIds.RightHand)
            {
                var otherHandSlotId = slotId == HandSlotIds.LeftHand
                    ? HandSlotIds.RightHand
                    : HandSlotIds.LeftHand;

                if (item is Weapon weapon && weapon.IsTwoHanded)
                {
                    TriggerSlotChangeEvent(null, otherHandSlotId);
                    slotsToSend.Add(otherHandSlotId);
                }
                else
                {
                    var itemInOtherHand = GetItemInSlot(otherHandSlotId);
                    if (itemInOtherHand is Weapon otherWeapon && otherWeapon.IsTwoHanded)
                    {
                        TriggerSlotChangeEvent(null, otherHandSlotId);
                        slotsToSend.Add(otherHandSlotId);
                    }
                }
            }

            return (wasEquipped, slotsToSend);
        }

        protected void TriggerSlotChangeEvent(string itemId, string slotId)
        {
            var eventArgs = new SlotChangeEventArgs(this, _livingEntity, slotId, itemId);
            _eventBus.PublishAsync(SlotChangeEventId, eventArgs).Forget();
        }

        public static UniTask DefaultHandlerForSlotChangeEventAsync(SlotChangeEventArgs eventArgs)
        {
            eventArgs.Inventory.ApplyEquippedItemChange(eventArgs.ItemId, eventArgs.SlotId);
            return UniTask.CompletedTask;
        }

        protected abstract void ApplyEquippedItemChange(string itemId, string slotId);

        protected abstract void ApplyEquippedItemChanges(Dictionary<string, string> equippedItems);

        protected abstract void NotifyOfItemsAdded(IEnumerable<ItemBase> itemsAdded);

        protected abstract void NotifyOfInventoryFull();

        protected abstract void NotifyOfItemsRemoved(IEnumerable<ItemBase> itemsRemoved);

        public void ToggleEquippedItemVisuals(string slotId, bool show)
        {
            if (!_equippedItems.ContainsKey(slotId)
                || _equippedItems[slotId].GameObject == null)
            {
                return;
            }

            _equippedItems[slotId].GameObject.SetActive(show);
        }

        public InventoryData GetInventoryData()
        {
            var changes = new InventoryData
            {
                CharacterId = _characterId,
                Items = _items.Select(x => _itemFactory.GetDataFromItem(_characterId, x.Value)).ToList(),
                EquippedItems = _equippedItems.ToDictionary(x => x.Key, x => x.Value.Item?.Id)
            };

            _isDirty = false;
            foreach (var kvp in _items.Where(x => x.Value.IsDirty))
            {
                kvp.Value.IsDirty = false;
            }

            return changes;
        }

        protected void MarkAsDirtyAndAddToQueue()
        {
            if (!IsServer || !_hasInventoryLoaded || IsDirty)
            {
                return;
            }

            // todo: zzz v0.6 - set debug log level
            Debug.Log($"Marking inventory as dirty for '{_characterId}'");

            _isDirty = true;
            _saveManager.AddToQueue(_characterId, this);
        }
    }
}
