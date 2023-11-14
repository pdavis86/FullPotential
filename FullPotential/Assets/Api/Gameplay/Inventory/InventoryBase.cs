using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedMemberHierarchy.Global

namespace FullPotential.Api.Gameplay.Inventory
{
    public abstract class InventoryBase : NetworkBehaviour, IInventory
    {
        private int _armorSlotCount;

        #region Protected variables
        // ReSharper disable InconsistentNaming

        protected Dictionary<string, ItemBase> _items;
        protected int _maxItemCount;
        protected Dictionary<SlotGameObjectName, EquippedItem> _equippedItems;

        //Services
        protected ITypeRegistry _typeRegistry;
        protected ILocalizer _localizer;

        // ReSharper restore InconsistentNaming
        #endregion

        #region Unity Events Handlers

        protected virtual void Awake()
        {
            _items = new Dictionary<string, ItemBase>();
            _equippedItems = new Dictionary<SlotGameObjectName, EquippedItem>();

            _armorSlotCount = Enum.GetNames(typeof(ArmorType)).Length;

            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        #endregion

        public int GetDefenseValue()
        {
            var defenseSum = 0;

            foreach (SlotGameObjectName slotGameObjectName in Enum.GetValues(typeof(SlotGameObjectName)))
            {
                var equippedItemId = _equippedItems.TryGetValue(slotGameObjectName, out var equippedItem)
                    ? equippedItem.Item?.Id
                    : null;

                if (equippedItemId.IsNullOrWhiteSpace())
                {
                    continue;
                }

                var item = GetItemWithId<ItemBase>(equippedItemId);
                if (item is IDefensible defensibleItem)
                {
                    defenseSum += defensibleItem.GetDefenseValue();
                }
            }

            return (int)Math.Floor((float)defenseSum / _armorSlotCount);
        }

        public T GetItemWithId<T>(string id, bool logIfNotFound = true) where T : ItemBase
        {
            var item = _items.FirstOrDefault(x => x.Value.Id == id).Value;

            if (item == null)
            {
                if (logIfNotFound)
                {
                    Debug.LogError($"Could not find the item with ID '{id}'");
                }
                return null;
            }

            if (item is not T castAsType)
            {
                throw new Exception($"Item '{id}' was not of the correct type: {typeof(T).Name}");
            }

            return castAsType;
        }

        public ItemBase GetItemInSlot(SlotGameObjectName slotGameObjectName)
        {
            return _equippedItems.TryGetValue(slotGameObjectName, out var equippedItem)
                ? equippedItem.Item
                : null;
        }

        public ItemStack TakeItemStack(string typeId, int maxSize)
        {
            var matches = _items
                .Where(
                    i => i.Value is ItemStack itemStack
                    && itemStack.RegistryTypeId == typeId)
                .Select(i => (ItemStack)i.Value)
                .OrderByDescending(i => i.Count);

            if (!matches.Any())
            {
                return null;
            }

            var returnStack = new ItemStack
            {
                Id = Guid.NewGuid().ToMinimisedString(),
                RegistryTypeId = matches.First().RegistryTypeId,
                RegistryType = matches.First().RegistryType
            };

            var countRemaining = maxSize;

            foreach (var itemStack in matches)
            {
                if (countRemaining >= itemStack.Count)
                {
                    returnStack.Count += itemStack.Count;
                    countRemaining -= itemStack.Count;
                    _items.Remove(itemStack.Id);
                    continue;
                }

                returnStack.Count += countRemaining;

                itemStack.Count -= countRemaining;

                if (itemStack.Count == 0)
                {
                    _items.Remove(itemStack.Id);
                }
            }

            return returnStack;
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

        public bool HasTypeEquipped(SlotGameObjectName slotGameObjectName)
        {
            return _equippedItems.ContainsKey(slotGameObjectName);
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

        protected void FillTypesFromIds(ItemBase item)
        {
            if (!string.IsNullOrWhiteSpace(item.RegistryTypeId) && item.RegistryType == null)
            {
                item.RegistryType = _typeRegistry.GetRegisteredForItem(item);
            }

            if (item is ItemWithTargetingAndShapeBase magicalItem && !string.IsNullOrWhiteSpace(magicalItem.TargetingTypeId))
            {
                magicalItem.Targeting = _typeRegistry.GetRegisteredTypes<ITargeting>()
                    .First(x => x.TypeId.ToString() == magicalItem.TargetingTypeId);

                magicalItem.TargetingVisuals = _typeRegistry.GetRegisteredTypes<ITargetingVisuals>()
                    .FirstOrDefault(v => v.TypeId.ToString() == magicalItem.TargetingVisualsTypeId);

                if (!string.IsNullOrWhiteSpace(magicalItem.ShapeTypeId))
                {
                    magicalItem.Shape = _typeRegistry.GetRegisteredTypes<IShape>()
                        .First(x => x.TypeId.ToString() == magicalItem.ShapeTypeId);

                    magicalItem.ShapeVisuals = _typeRegistry.GetRegisteredTypes<IShapeVisuals>()
                        .FirstOrDefault(v => v.TypeId.ToString() == magicalItem.ShapeVisualsTypeId);
                }
            }

            if (item is ItemForCombatBase combatItem)
            {
                if (combatItem.EffectIds != null && combatItem.EffectIds.Length > 0 && combatItem.Effects == null)
                {
                    combatItem.Effects = combatItem.EffectIds.Select(x => _typeRegistry.GetEffect(new Guid(x))).ToList();
                }
            }
        }

        protected void MergeItemStacks(ItemStack itemStack)
        {
            var partiallyFullStacks = _items
                .Where(i => i.Value is ItemStack ist && ist.Count < ist.MaxSize)
                .Select(i => (ItemStack)i.Value);

            if (!partiallyFullStacks.Any())
            {
                _items.Add(itemStack.Id, itemStack);
                return;
            }

            var itemsRemaining = itemStack.Count;

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
                break;
            }

            if (itemsRemaining <= 0)
            {
                return;
            }

            itemStack.Count = itemsRemaining;
            _items.Add(itemStack.Id, itemStack);
        }
    }
}
