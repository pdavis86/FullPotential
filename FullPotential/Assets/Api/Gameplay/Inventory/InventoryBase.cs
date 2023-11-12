﻿using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;
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
        protected Dictionary<SlotGameObjectName, EquippedItem> _equippedItems;

        // ReSharper restore InconsistentNaming
        #endregion

        #region Unity Events Handlers

        protected virtual void Awake()
        {
            _items = new Dictionary<string, ItemBase>();
            _equippedItems = new Dictionary<SlotGameObjectName, EquippedItem>();

            _armorSlotCount = Enum.GetNames(typeof(ArmorType)).Length;
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
