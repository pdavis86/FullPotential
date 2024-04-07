using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Data;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.EventArgs;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Networking;
using FullPotential.Api.Obsolete.Networking;
using FullPotential.Api.Obsolete.Networking.Data;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Ui;
using FullPotential.Api.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedMemberHierarchy.Global
// ReSharper disable MemberCanBePrivate.Global

namespace FullPotential.Api.Gameplay.Behaviours
{
    public abstract class InventoryBase : NetworkBehaviour
    {
        public const string EventIdSlotChange = "9c7972de-4136-4825-aaa3-11925ad049ee";

        private IFragmentedMessageReconstructor _inventoryChangesReconstructor;

        #region Protected variables
        // ReSharper disable InconsistentNaming

        protected Dictionary<string, ItemBase> _items;
        protected int _maxItemCount;
        protected Dictionary<string, EquippedItem> _equippedItems;
        protected LivingEntityBase _livingEntity;

        //Services
        protected ITypeRegistry _typeRegistry;
        protected ILocalizer _localizer;
        protected IRpcService _rpcService;
        private IEventManager _eventManager;

        // ReSharper restore InconsistentNaming
        #endregion

        #region Unity Events Handlers

        protected virtual void Awake()
        {
            _items = new Dictionary<string, ItemBase>();
            _equippedItems = new Dictionary<string, EquippedItem>();

            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();
            _eventManager = DependenciesContext.Dependencies.GetService<IEventManager>();

            _inventoryChangesReconstructor = DependenciesContext.Dependencies.GetService<IFragmentedMessageReconstructorFactory>().Create();

            _livingEntity = GetComponent<LivingEntityBase>();
        }

        #endregion

        #region RPC Calls

        public void SendInventoryChangesToClient(InventoryChanges changes)
        {
            if (IsHost && OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                return;
            }

            SendInventoryChangesToClients(changes, _rpcService.ForPlayer(OwnerClientId));
        }

        protected void SendInventoryChangesToClients(InventoryChanges changes, ClientRpcParams rpcParams)
        {
            if (!IsServer)
            {
                return;
            }

            foreach (var message in _inventoryChangesReconstructor.GetFragmentedMessages(changes))
            {
                HandleChangeMessageFragmentClientRpc(message, rpcParams);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void HandleChangeMessageFragmentClientRpc(string fragmentedMessageJson, ClientRpcParams clientRpcParams)
        {
            var fragmentedMessage = JsonUtility.FromJson<FragmentedMessage>(fragmentedMessageJson);

            _inventoryChangesReconstructor.AddMessage(fragmentedMessage);
            if (!_inventoryChangesReconstructor.HaveAllMessages(fragmentedMessage.GroupId))
            {
                return;
            }

            var changes = JsonUtility.FromJson<InventoryChanges>(_inventoryChangesReconstructor.Reconstruct(fragmentedMessage.GroupId));
            ApplyInventoryChanges(changes, true);
        }

        #endregion

        public bool ApplyInventoryChanges(InventoryChanges changes, bool isFromClientRpc = false)
        {
            if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
            {
                var itemsRemoved = new List<ItemBase>();
                foreach (var id in changes.IdsToRemove)
                {
                    if (!_items.ContainsKey(id))
                    {
                        Debug.LogWarning($"Could not remove item with ID {id}. Was this admin crafting?");
                        continue;
                    }

                    itemsRemoved.Add(_items[id]);
                    _items.Remove(id);
                }

                NotifyOfItemsRemoved(itemsRemoved);
            }

            //todo: zzz v0.6 - can still take item stacks when inventory is full if there is space
            if (IsInventoryFull())
            {
                NotifyOfInventoryFull();
                return false;
            }

            var nonItemStacks = changes.GetNonItemStacks().ToList();

            var itemsToAdd = new List<ItemBase>();

            foreach (var item in nonItemStacks)
            {
                FillTypesFromIds(item);

                if (_items.ContainsKey(item.Id))
                {
                    UpdateExistingItem(item);
                }
                else
                {
                    _items.Add(item.Id, item);
                    itemsToAdd.Add(item);
                }
            }

            if (changes.ItemStacks != null && changes.ItemStacks.Any())
            {
                foreach (var itemStack in changes.ItemStacks)
                {
                    FillTypesFromIds(itemStack);

                    if (IsServer)
                    {
                        var newStack = MergeItemStacks(itemStack);
                        if (newStack != null)
                        {
                            itemsToAdd.Add(newStack);
                        }
                    }
                    else if (_items.ContainsKey(itemStack.Id))
                    {
                        UpdateExistingItem(itemStack);
                    }
                    else
                    {
                        _items.Add(itemStack.Id, itemStack);
                        itemsToAdd.Add(itemStack);
                    }
                }
            }

            NotifyOfItemsAdded(itemsToAdd);

            ApplyEquippedItemChanges(changes.EquippedItems);

            if (!isFromClientRpc)
            {
                SendInventoryChangesToClient(changes);
            }

            return true;
        }

        private void UpdateExistingItem(ItemBase newItem)
        {
            var newJson = JsonUtility.ToJson(newItem);
            var oldItem = _items[newItem.Id];
            JsonUtility.FromJsonOverwrite(newJson, oldItem);
        }

        private T CastItemAsType<T>(ItemBase item, bool logIfNotFound, string identifierName, string id) where T : ItemBase
        {
            if (item == null)
            {
                if (logIfNotFound)
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

        public T GetItemWithId<T>(string id, bool logIfNotFound = true) where T : ItemBase
        {
            var item = _items.FirstOrDefault(x => x.Value.Id == id).Value;
            return CastItemAsType<T>(item, logIfNotFound, "ID", id);
        }

        public T GetItemInSlot<T>(string slotId, bool logIfNotFound = true) where T : ItemBase
        {
            var item = GetItemInSlot(slotId);
            return CastItemAsType<T>(item, logIfNotFound, "slot ID", slotId);
        }

        public ItemBase GetItemInSlot(string slotId)
        {
            return _equippedItems.TryGetValue(slotId, out var equippedItem)
                ? equippedItem.Item
                : null;
        }

        public (int countTaken, InventoryChanges invChanges) TakeCountFromItemStacks(string typeId, int count)
        {
            if (!IsServer)
            {
                Debug.LogError("TakeCountFromItemStacks called client-side");
                return (0, null);
            }

            var matches = _items
                .Where(
                    i => i.Value is ItemStack itemStack
                    && itemStack.RegistryTypeId == typeId)
                .Select(i => (ItemStack)i.Value)
                .OrderBy(i => i.Count);

            if (!matches.Any())
            {
                return (0, null);
            }

            var stacksChanged = new List<ItemStack>();
            var idsToRemove = new List<string>();
            var countRemaining = count;

            foreach (var itemStack in matches)
            {
                if (countRemaining >= itemStack.Count)
                {
                    countRemaining -= itemStack.Count;
                    _items.Remove(itemStack.Id);
                    idsToRemove.Add(itemStack.Id);
                    continue;
                }

                itemStack.Count -= countRemaining;
                countRemaining = 0;

                if (itemStack.Count == 0)
                {
                    _items.Remove(itemStack.Id);
                }
                else
                {
                    stacksChanged.Add(itemStack);
                }

                break;
            }

            var countTaken = count - countRemaining;

            var invChanges = new InventoryChanges
            {
                IdsToRemove = idsToRemove.ToArray(),
                ItemStacks = stacksChanged.ToArray()
            };

            return (countTaken, invChanges);
        }

        public int GetItemStackTotal(string typeId)
        {
            return _items
                .Where(
                    i => i.Value is ItemStack itemStack
                    && itemStack.RegistryTypeId == typeId)
                .Select(i => (ItemStack)i.Value)
                .Sum(i => i.Count);
        }

        public bool IsInventoryFull()
        {
            return _items.Count >= _maxItemCount;
        }

        public List<ItemForCombatBase> GetComponentsFromIds(string[] componentIds)
        {
            //Check that the components are actually in the player's inventory and load them in the order they are given
            var components = new List<ItemForCombatBase>();
            foreach (var id in componentIds)
            {
                var match = GetItemWithId<ItemForCombatBase>(id);
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

            return _typeRegistry.GetRegisteredTypes<IArmor>().FirstOrDefault(t => t.TypeId.ToString() == slotId) != null
                   || _typeRegistry.GetRegisteredTypes<IAccessory>().FirstOrDefault(t => slotId.StartsWith(t.TypeId.ToString())) != null
                   || _typeRegistry.GetRegisteredTypes<IRegisterableWithSlot>().FirstOrDefault(t => t.TypeId.ToString() == slotId) != null;
        }

        protected void FillTypesFromIds(ItemBase item)
        {
            if (!string.IsNullOrWhiteSpace(item.RegistryTypeId) && item.RegistryType == null)
            {
                item.RegistryType = _typeRegistry.GetRegistryTypeForItem(item);
            }

            if (item is ItemWithTargetingAndShapeBase withTargetingAndShape && !string.IsNullOrWhiteSpace(withTargetingAndShape.TargetingTypeId))
            {
                withTargetingAndShape.Targeting = _typeRegistry.GetRegisteredTypes<ITargeting>()
                    .First(x => x.TypeId.ToString() == withTargetingAndShape.TargetingTypeId);

                withTargetingAndShape.TargetingVisuals = _typeRegistry.GetRegisteredTypes<ITargetingVisuals>()
                    .FirstOrDefault(v => v.TypeId.ToString() == withTargetingAndShape.TargetingVisualsTypeId);

                if (!string.IsNullOrWhiteSpace(withTargetingAndShape.ShapeTypeId))
                {
                    withTargetingAndShape.Shape = _typeRegistry.GetRegisteredTypes<IShape>()
                        .First(x => x.TypeId.ToString() == withTargetingAndShape.ShapeTypeId);

                    withTargetingAndShape.ShapeVisuals = _typeRegistry.GetRegisteredTypes<IShapeVisuals>()
                        .FirstOrDefault(v => v.TypeId.ToString() == withTargetingAndShape.ShapeVisualsTypeId);
                }
            }

            if (item is Consumer consumer)
            {
                consumer.ResourceType = _typeRegistry.GetRegisteredTypes<IResource>()
                    .First(x => x.TypeId.ToString() == consumer.ResourceTypeId);
            }
            else if (item is SpecialGear specialGear)
            {
                specialGear.ResourceType = _typeRegistry.GetRegisteredTypes<IResource>()
                    .First(x => x.TypeId.ToString() == specialGear.ResourceTypeId);
            }

            if (item is IHasItemVisuals itemWithVisuals)
            {
                switch (item)
                {
                    case Weapon:
                        SetItemVisuals<IWeaponVisuals>(itemWithVisuals, item);
                        break;

                    case Armor:
                        SetItemVisuals<IArmorVisuals>(itemWithVisuals, item);
                        break;

                    case Accessory:
                        SetItemVisuals<IAccessoryVisuals>(itemWithVisuals, item);
                        break;

                    case SpecialGear:
                        SetItemVisuals<ISpecialGearVisuals>(itemWithVisuals, item);
                        break;
                }
            }

            if (item is ItemForCombatBase combatItem)
            {
                if (combatItem.EffectIds != null && combatItem.EffectIds.Length > 0 && combatItem.Effects == null)
                {
                    combatItem.Effects = combatItem.EffectIds.Select(x => _typeRegistry.GetEffect(x)).ToList();
                }

                //For backwards compatibility
                combatItem.Effects ??= new List<IEffect>();
                if (!combatItem.Effects.Any())
                {
                    combatItem.Effects.Add(_typeRegistry.GetEffect(EffectTypeIds.HurtId));
                }

                var allEffectComputations = _typeRegistry.GetRegisteredTypes<IEffectComputation>();
                foreach (var effect in combatItem.Effects)
                {
                    var computation = allEffectComputations.FirstOrDefault(x => x.EffectTypeId == effect.TypeId.ToString());
                    if (computation != null)
                    {
                        combatItem.MainEffectComputation = computation;
                        break;
                    }
                }
            }
        }

        private void SetItemVisuals<T>(IHasItemVisuals itemWithVisuals, ItemBase item)
            where T : IItemVisuals
        {
            if (itemWithVisuals.VisualsTypeId.IsNullOrWhiteSpace())
            {
                itemWithVisuals.Visuals = _typeRegistry.GetRegisteredTypes<T>()
                    .FirstOrDefault(v => v.ApplicableToTypeIdString == item.RegistryType.TypeId.ToString());
            }
            else
            {
                itemWithVisuals.Visuals = _typeRegistry.GetRegisteredTypes<T>()
                    .FirstOrDefault(v => v.TypeId.ToString() == itemWithVisuals.VisualsTypeId);
            }
        }

        private ItemStack MergeItemStacks(ItemStack newStack)
        {
            if (!IsServer)
            {
                Debug.LogError("MergeItemStacks called client-side");
                return null;
            }

            var partiallyFullStacks = _items
                .Where(
                    i => i.Value is ItemStack oldStack
                    && oldStack.RegistryTypeId == newStack.RegistryTypeId
                    && oldStack.Count < oldStack.MaxSize)
                .Select(i => (ItemStack)i.Value);

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

        public void PopulateInventoryChangesWithItem(InventoryChanges invChanges, ItemBase item)
        {
            var itemType = item.GetType();
            invChanges.Accessories = itemType == typeof(Accessory) ? new[] { item as Accessory } : null;
            invChanges.Armor = itemType == typeof(Armor) ? new[] { item as Armor } : null;
            invChanges.Consumers = itemType == typeof(Consumer) ? new[] { item as Consumer } : null;
            invChanges.Weapons = itemType == typeof(Weapon) ? new[] { item as Weapon } : null;
            invChanges.ItemStacks = itemType == typeof(ItemStack) ? new[] { item as ItemStack } : null;
            invChanges.SpecialGear = itemType == typeof(SpecialGear) ? new[] { item as SpecialGear } : null;
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
                TriggerSlotChangeEvent(item, slotId);
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

        protected void TriggerSlotChangeEvent(ItemBase item, string slotId)
        {
            var eventArgs = new SlotChangeEventArgs(this, _livingEntity, slotId, item?.Id);
            _eventManager.Trigger(EventIdSlotChange, eventArgs);
        }

        public static void DefaultHandlerForSlotChangeEvent(IEventHandlerArgs eventArgs)
        {
            var slotChangeEventArgs = (SlotChangeEventArgs)eventArgs;
            slotChangeEventArgs.Inventory.SetEquippedItem(slotChangeEventArgs.ItemId, slotChangeEventArgs.SlotId);
        }

        protected abstract void SetEquippedItem(string itemId, string slotId);

        protected abstract void ApplyEquippedItemChanges(SerializableKeyValuePair<string, string>[] equippedItems);

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
    }
}
