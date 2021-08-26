using Assets.ApiScripts.Registry;
using Assets.Core.Data;
using Assets.Core.Extensions;
using Assets.Core.Registry.Base;
using Assets.Core.Registry.Types;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ConvertToUsingDeclaration

namespace Assets.Core.Storage
{
    public class PlayerInventory
    {
        private readonly PlayerState _playerState;
        private readonly List<ItemBase> _items;
        private readonly int _slotCount;
        private readonly GameObject[] _equippedObjects;

        private int _maxItems;
        private string[] _equipSlots;

        //todo: Cache effective stats based on armour

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

        public int GetSlotCount()
        {
            return _slotCount;
        }

        public PlayerInventory(PlayerState playerState)
        {
            _playerState = playerState;

            _items = new List<ItemBase>();

            _slotCount = Enum.GetNames(typeof(SlotIndexToGameObjectName)).Length;
            _equipSlots = new string[_slotCount];
            _equippedObjects = new GameObject[_slotCount];
        }

        public IEnumerable<ItemBase> GetItemsForSlotId(IGear.GearSlot? inventorySlot)
        {
            return _items.Where(x =>
                inventorySlot == null
                || (x is Accessory acc && (int)((IGearAccessory)acc.RegistryType).InventorySlot == (int)inventorySlot)
                || (x is Armor armor && (int)((IGearArmor)armor.RegistryType).InventorySlot == (int)inventorySlot)
                || ((x is Weapon || x is Spell) && inventorySlot == IGear.GearSlot.Hand)
            );
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

                var addedItems = Enumerable.Empty<ItemBase>()
                    .UnionIfNotNull(changes.Loot)
                    .UnionIfNotNull(changes.Accessories)
                    .UnionIfNotNull(changes.Armor)
                    .UnionIfNotNull(changes.Spells)
                    .UnionIfNotNull(changes.Weapons);

                _items.AddRange(addedItems);

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

                if (changes.EquipSlots != null && changes.EquipSlots.Any())
                {
                    if (changes.EquipSlots.Length == _equipSlots.Length)
                    {
                        _equipSlots = changes.EquipSlots;
                    }
                    else
                    {
                        HandleOldSaveFile(changes);
                    }

                    //DebugLogEquippedItems();
                }

                EquipItems();

                if (!firstSetup)
                {
                    var addedItemsCount = addedItems.Count();

                    if (addedItemsCount == 1)
                    {
                        var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemadded");
                        _playerState.ShowAlertForItemsAddedToInventory(string.Format(alertText, addedItems.First().Name));
                    }
                    else
                    {
                        var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemsadded");
                        _playerState.ShowAlertForItemsAddedToInventory(string.Format(alertText, addedItemsCount));
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
        }

        public ItemBase GetItemInSlot(int slotIndex)
        {
            var itemId = _equipSlots[slotIndex];
            return !string.IsNullOrWhiteSpace(itemId)
                ? GetItemWithId<ItemBase>(itemId)
                : null;
        }

        public bool IsEquipped(string id)
        {
            return _equipSlots.Contains(id);
        }

        private void HandleOldSaveFile(Inventory changes)
        {
            if (changes.EquipSlots.Length != _equipSlots.Length)
            {
                Debug.LogWarning("Incoming EquipSlots length differed to existing");

                for (var i = 0; i < Math.Min(_equipSlots.Length, changes.EquipSlots.Length); i++)
                {
                    _equipSlots[i] = changes.EquipSlots[i];
                }
            }
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
                EquipSlots = _equipSlots
            };
        }

        //private void AddOfType<T>(string stringValue) where T : ItemBase
        //{
        //    Add(JsonUtility.FromJson<T>(stringValue));
        //    //Debug.Log($"Inventory now has {Items.Count} items in it");
        //}

        public T GetItemWithId<T>(string id) where T : ItemBase
        {
            var item = _items.FirstOrDefault(x => x.Id == id);

            if (!(item is T))
            {
                throw new Exception($"Item '{id}' was not of the correct type: {typeof(T).Name}");
            }

            return item as T;
        }

        public void Add(ItemBase item)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogError("Inventory Add called on the client!");
                return;
            }

            if (_items.Count == _maxItems)
            {
                _playerState.AlertInventoryIsFull();
                return;
            }

            //todo: what type is the item being added?
            var change = new InventoryAndRemovals { Loot = new[] { item as Loot } };

            ApplyInventoryAndRemovals(change);

            var json = JsonUtility.ToJson(change);
            var stream = PooledNetworkBuffer.Get();
            using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
            {
                writer.WriteString(json);
                CustomMessagingManager.SendNamedMessage(nameof(Networking.MessageType.InventoryChange), _playerState.OwnerClientId, stream);
            }
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

        public void SetItemToSlot(string slotName, string itemId)
        {
            if (!Enum.TryParse<SlotIndexToGameObjectName>(slotName, out var slotResult))
            {
                Debug.LogError($"Failed to find slot for name {slotName}");
                return;
            }

            var equippedIndex = Array.IndexOf(_equipSlots, itemId);
            if (equippedIndex >= 0)
            {
                //Debug.Log($"{itemId} is already assigned to slot {equippedIndex}");
                //EquipSlots[equippedIndex] = null;
                EquipItem(equippedIndex, null);
            }

            //EquipSlots[(int)slotResult] = itemId;
            EquipItem((int)slotResult, itemId);
        }

        public Spell GetSpellInHand(bool leftHand)
        {
            var itemId = leftHand
                ? _equipSlots[(int)SlotIndexToGameObjectName.LeftHand]
                : _equipSlots[(int)SlotIndexToGameObjectName.RightHand];

            var item = _items.FirstOrDefault(x => x.Id == itemId);

            return item as Spell;
        }

        private void SpawnItemInHand(int index, ItemBase item, bool leftHand = true)
        {
            if (item is Weapon weapon)
            {
                var registryType = item.RegistryType as IGearWeapon;

                if (registryType == null)
                {
                    Debug.LogError("Weapon did not have a RegistryType");
                    return;
                }

                GameManager.Instance.TypeRegistry.LoadAddessable(
                    weapon.IsTwoHanded ? registryType.PrefabAddressTwoHanded : registryType.PrefabAddress,
                    prefab =>
                    {
                        var weaponGo = UnityEngine.Object.Instantiate(prefab, _playerState.InFrontOfPlayer.transform);
                        weaponGo.transform.localEulerAngles = new Vector3(0, 90);
                        weaponGo.transform.localPosition = new Vector3(leftHand ? -0.38f : 0.38f, -0.25f, 1.9f);

                        Assets.Helpers.GameObjectHelper.SetGameLayerRecursive(weaponGo, _playerState.InFrontOfPlayer.layer);

                        _equippedObjects[index] = weaponGo;
                    }
                );
            }
            else
            {
                //todo: implement other items
                Debug.LogWarning($"Not implemented SpawnItemInHand handling for item type {item.GetType().Name} yet");
            }
        }

        private void EquipItem(int slotIndex, string itemId)
        {
            var currentlyInGame = _equippedObjects[slotIndex];
            if (currentlyInGame != null)
            {
                UnityEngine.Object.Destroy(currentlyInGame);
            }

            if (string.IsNullOrWhiteSpace(itemId))
            {
                _equipSlots[slotIndex] = null;
                return;
            }

            _equipSlots[slotIndex] = itemId;

            if (slotIndex == (int)SlotIndexToGameObjectName.LeftHand)
            {
                SpawnItemInHand(slotIndex, _items.First(x => x.Id == itemId));
            }
            else if (slotIndex == (int)SlotIndexToGameObjectName.RightHand)
            {
                SpawnItemInHand(slotIndex, _items.First(x => x.Id == itemId), false);
            }
        }

        private void EquipItems()
        {
            for (var i = 0; i < _equipSlots.Length; i++)
            {
                EquipItem(i, _equipSlots[i]);
            }
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
