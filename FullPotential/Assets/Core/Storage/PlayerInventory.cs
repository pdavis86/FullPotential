using FullPotential.Assets.Api.Registry;
using FullPotential.Assets.Core.Data;
using FullPotential.Assets.Core.Extensions;
using FullPotential.Assets.Core.Registry.Base;
using FullPotential.Assets.Core.Registry.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ConvertToUsingDeclaration

namespace FullPotential.Assets.Core.Storage
{
    public class PlayerInventory
    {
        public enum SlotIndexToGameObjectName
        {
            Helm,
            Chest,
            Legs,
            Feet,
            Barrier,
            LeftHand,
            RightHand,
            LeftRing,
            RightRing,
            Belt,
            Amulet
        }

        public readonly string[] EquipSlots;
        public readonly GameObject[] EquippedObjects;

        private readonly PlayerState _playerState;
        private readonly List<ItemBase> _items;
        private readonly int _slotCount;

        private int _maxItems;

        public PlayerInventory(PlayerState playerState)
        {
            _playerState = playerState;

            _items = new List<ItemBase>();

            _slotCount = Enum.GetNames(typeof(SlotIndexToGameObjectName)).Length;
            EquipSlots = new string[_slotCount];
            EquippedObjects = new GameObject[_slotCount];
        }

        public IEnumerable<ItemBase> GetCompatibleItemsForSlot(IGear.GearSlot? inventorySlot)
        {
            return _items.Where(x =>
                inventorySlot == null
                || (x is Accessory acc && (int)((IGearAccessory)acc.RegistryType).InventorySlot == (int)inventorySlot)
                || (x is Armor armor && (int)((IGearArmor)armor.RegistryType).InventorySlot == (int)inventorySlot)
                || ((x is Weapon || x is Spell) && inventorySlot == IGear.GearSlot.Hand)
            );
        }

        public int GetSlotCount()
        {
            return _slotCount;
        }

        public void ApplyInventoryAndRemovals(InventoryAndRemovals changes)
        {
            if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
            {
                var itemsRemoved = _items.RemoveAll(x => changes.IdsToRemove.Contains(x.Id));
                _playerState.AlertOfInventoryRemovals(itemsRemoved);
            }

            ApplyInventory(changes);
        }

        public bool ApplyInventory(Inventory changes, bool firstSetup = false)
        {
            try
            {
                if (changes == null)
                {
                    //Debug.Log("No inventory changes supplied");
                    return true;
                }

                if (changes.MaxItems > 0)
                {
                    _maxItems = changes.MaxItems;
                }

                if (_items.Count == _maxItems)
                {
                    _playerState.AlertInventoryIsFull();
                }

                var addedItems = Enumerable.Empty<ItemBase>()
                    .UnionIfNotNull(changes.Loot)
                    .UnionIfNotNull(changes.Accessories)
                    .UnionIfNotNull(changes.Armor)
                    .UnionIfNotNull(changes.Spells)
                    .UnionIfNotNull(changes.Weapons);

                _items.AddRange(addedItems);

                FillTypesFromIds();

                if (changes.EquipSlots != null && changes.EquipSlots.Any())
                {
                    if (changes.EquipSlots.Length == EquipSlots.Length)
                    {
                        for (var i = 0; i < EquipSlots.Length; i++)
                        {
                            EquipItem(i, changes.EquipSlots[i]);
                        }
                    }
                    else
                    {
                        //HandleOldSaveFile
                        if (changes.EquipSlots.Length != EquipSlots.Length)
                        {
                            Debug.LogWarning("Incoming EquipSlots length differed to existing");

                            for (var i = 0; i < Math.Min(EquipSlots.Length, changes.EquipSlots.Length); i++)
                            {
                                EquipSlots[i] = changes.EquipSlots[i];
                            }
                        }
                    }
                }

                if (!firstSetup)
                {
                    var addedItemsCount = addedItems.Count();

                    if (addedItemsCount == 1)
                    {
                        var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemadded");
                        _playerState.ShowAlertForItemsAddedToInventory(string.Format(alertText, addedItems.First().Name));
                    }
                    else if (addedItemsCount > 1)
                    {
                        var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemsadded");
                        _playerState.ShowAlertForItemsAddedToInventory(string.Format(alertText, addedItemsCount));
                    }
                }

                _playerState.SpawnEquippedObjects();

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
        }

        private void FillTypesFromIds()
        {
            foreach (var item in _items)
            {
                if (!string.IsNullOrWhiteSpace(item.RegistryTypeId) && item.RegistryType == null)
                {
                    item.RegistryType = GameManager.Instance.TypeRegistry.GetRegisteredForItem(item);
                }

                if (item is MagicalItemBase magicalItem)
                {
                    if (!string.IsNullOrWhiteSpace(magicalItem.ShapeTypeName))
                    {
                        magicalItem.Shape = GameManager.Instance.ResultFactory.GetSpellShape(magicalItem.ShapeTypeName);
                    }
                    if (!string.IsNullOrWhiteSpace(magicalItem.TargetingTypeName))
                    {
                        magicalItem.Targeting = GameManager.Instance.ResultFactory.GetSpellTargeting(magicalItem.TargetingTypeName);
                    }
                }

                if (item.EffectIds != null && item.EffectIds.Length > 0 && item.Effects == null)
                {
                    item.Effects = item.EffectIds.Select(x => GameManager.Instance.TypeRegistry.GetEffect(new Guid(x))).ToList();
                }
            }
        }

        public ItemBase GetItemInSlot(int slotIndex)
        {
            var itemId = EquipSlots[slotIndex];
            return !string.IsNullOrWhiteSpace(itemId)
                ? GetItemWithId<ItemBase>(itemId)
                : null;
        }

        public Inventory GetDataForOtherPlayers()
        {
            var groupedItems = _items
                .Where(x => EquipSlots.Contains(x.Id))
                .GroupBy(x => x.GetType());

            return new Inventory
            {
                Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(Loot))?.Select(x => x as Loot).ToArray(),
                Accessories = groupedItems.FirstOrDefault(x => x.Key == typeof(Accessory))?.Select(x => x as Accessory).ToArray(),
                Armor = groupedItems.FirstOrDefault(x => x.Key == typeof(Armor))?.Select(x => x as Armor).ToArray(),
                Spells = groupedItems.FirstOrDefault(x => x.Key == typeof(Spell))?.Select(x => x as Spell).ToArray(),
                Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray(),
                EquipSlots = EquipSlots
            };
        }

        public bool IsEquipped(string id)
        {
            return EquipSlots.Contains(id);
        }

        public Inventory GetSaveData()
        {
            //todo: feasible to use separate lists for different types of item?
            var groupedItems = _items.GroupBy(x => x.GetType());

            return new Inventory
            {
                MaxItems = _maxItems,
                Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(Loot))?.Select(x => x as Loot).ToArray(),
                Accessories = groupedItems.FirstOrDefault(x => x.Key == typeof(Accessory))?.Select(x => x as Accessory).ToArray(),
                Armor = groupedItems.FirstOrDefault(x => x.Key == typeof(Armor))?.Select(x => x as Armor).ToArray(),
                Spells = groupedItems.FirstOrDefault(x => x.Key == typeof(Spell))?.Select(x => x as Spell).ToArray(),
                Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray(),
                EquipSlots = EquipSlots
            };
        }

        public T GetItemWithId<T>(string id) where T : ItemBase
        {
            var item = _items.FirstOrDefault(x => x.Id == id);

            if (item == null)
            {
                Debug.LogError($"Could not find the item with ID '{id}'");
                return null;
            }

            if (!(item is T))
            {
                throw new Exception($"Item '{id}' was not of the correct type: {typeof(T).Name}");
            }

            return item as T;
        }

        //public void RemoveIds(IEnumerable<string> idEnumerable)
        //{
        //    foreach (var id in idEnumerable)
        //    {
        //        var matchingItem = _items.FirstOrDefault(x => x.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase));
        //        if (matchingItem == null)
        //        {
        //            Debug.LogError("No item found with ID: " + id);
        //            continue;
        //        }
        //        _items.Remove(matchingItem);
        //    }
        //}

        public List<ItemBase> GetComponentsFromIds(string[] componentIds)
        {
            //Check that the components are actually in the player's inventory and load them in the order they are given
            var components = new List<ItemBase>();
            foreach (var id in componentIds)
            {
                components.Add(_items.FirstOrDefault(x => x.Id == id));
            }
            return components;
        }

        public List<string> ValidateIsCraftable(string[] componentIds, ItemBase itemToCraft)
        {
            var components = GetComponentsFromIds(componentIds);

            var errors = new List<string>();
            if (itemToCraft is Spell spell)
            {
                if (spell.EffectIds.Length == 0)
                {
                    errors.Add(GameManager.Instance.Localizer.Translate("crafting.error.spellmissingeffect"));
                }
            }
            else if (itemToCraft is Weapon weapon)
            {
                if (components.Count > 8)
                {
                    errors.Add(GameManager.Instance.Localizer.Translate("crafting.error.toomanycomponents"));
                }
                if (components.Count > 4 && !weapon.IsTwoHanded)
                {
                    errors.Add(GameManager.Instance.Localizer.Translate("crafting.error.toomanyforonehanded"));
                }
            }

            return errors;
        }

        public Spell GetSpellInHand(bool leftHand)
        {
            var itemId = leftHand
                ? EquipSlots[(int)SlotIndexToGameObjectName.LeftHand]
                : EquipSlots[(int)SlotIndexToGameObjectName.RightHand];

            var item = _items.FirstOrDefault(x => x.Id == itemId);

            return item as Spell;
        }

        public void EquipItem(int slotIndex, string itemId)
        {
            //If necessary, un-equip from a different slot
            var equippedIndex = Array.IndexOf(EquipSlots, itemId);
            if (equippedIndex >= 0)
            {
                EquipSlots[equippedIndex] = null;
            }

            EquipSlots[slotIndex] = itemId;
        }





        //private void DebugLogEquippedItems()
        //{
        //    for (var i = 0; i < EquipSlots.Length; i++)
        //    {
        //        if (EquipSlots[i] == string.Empty)
        //        {
        //            continue;
        //        }

        //        var item = Items.FirstOrDefault(x => x.Id == EquipSlots[i]);
        //        var slotName = Enum.GetName(typeof(SlotIndexToGameObjectName), i);

        //        Debug.Log($"Equiped '{item?.Name}' to slot '{slotName}'");
        //    }
        //}

    }
}
