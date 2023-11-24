using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Gameplay.Inventory.EventArgs;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Networking;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.GameManagement.Inventory;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Player
{
    public class PlayerInventory : InventoryBase, IPlayerInventory
    {
        //Services
        private IRpcService _rpcService;
        private IInventoryDataService _inventoryDataService;
        private IEventManager _eventManager;

        private PlayerState _playerState;

        private readonly Dictionary<string, string> _itemIdToShapeMapping = new Dictionary<string, string>();
        private readonly FragmentedMessageReconstructor _inventoryChangesReconstructor = new FragmentedMessageReconstructor();

        #region Unity Events Handlers

        // ReSharper disable once UnusedMember.Local
        protected override void Awake()
        {
            base.Awake();

            _playerState = GetComponent<PlayerState>();

            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();
            _inventoryDataService = DependenciesContext.Dependencies.GetService<IInventoryDataService>();
            _eventManager = DependenciesContext.Dependencies.GetService<IEventManager>();
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void EquipItemServerRpc(string itemId, string slotId)
        {
            var item = _items[itemId];

            var slotChange = HandleSlotChange(item, slotId);

            var saveData = GetSaveData();

            GameManager.Instance.QueueAsapSave(_playerState.Username);

            var invChange = new InventoryChanges
            {
                EquippedItems = saveData.EquippedItems.Where(x => slotChange.SlotsToSend.Contains(x.Key)).ToArray()
            };

            if (slotChange.WasEquipped)
            {
                _inventoryDataService.PopulateInventoryChangesWithItem(invChange, item);
            }

            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            foreach (var message in FragmentedMessageReconstructor.GetFragmentedMessages(invChange))
            {
                ApplyEquipChangeClientRpc(message, nearbyClients);
            }
        }

        #endregion

        #region ClientRpc calls

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void ApplyEquipChangeClientRpc(string fragmentedMessageJson, ClientRpcParams clientRpcParams)
        {
            var fragmentedMessage = JsonUtility.FromJson<FragmentedMessage>(fragmentedMessageJson);

            _inventoryChangesReconstructor.AddMessage(fragmentedMessage);
            if (!_inventoryChangesReconstructor.HaveAllMessages(fragmentedMessage.GroupId))
            {
                return;
            }

            var changes = JsonUtility.FromJson<InventoryChanges>(_inventoryChangesReconstructor.Reconstruct(fragmentedMessage.GroupId));

            var equippedItem = Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(changes.Accessories)
                .UnionIfNotNull(changes.Armor)
                .UnionIfNotNull(changes.Consumers)
                .UnionIfNotNull(changes.Weapons)
                .FirstOrDefault();

            if (equippedItem != null && !_items.ContainsKey(equippedItem.Id))
            {
                FillTypesFromIds(equippedItem);
                _items.Add(equippedItem.Id, equippedItem);
            }

            foreach (var sourceKvp in changes.EquippedItems)
            {
                var item = sourceKvp.Value.IsNullOrWhiteSpace() ? null : _items[sourceKvp.Value];
                var slotId = sourceKvp.Key;

                TriggerSlotChangeEvent(item, slotId);
                SpawnEquippedObject(item, slotId);
            }

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                StartCoroutine(ResetEquipmentUi());
            }
            else if (!IsServer)
            {
                var keysToRemove = new List<string>();
                foreach (var kvp in _items)
                {
                    if (GetEquippedWithItemId(kvp.Key) == null)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                foreach (var key in keysToRemove)
                {
                    _items.Remove(key);
                }
            }

            _playerState.UpdateUiHealthAndDefenceValues();
        }

        #endregion

        //todo: zzz v0.5 - generalise for use in FighterBase
        public (bool WasEquipped, List<string> SlotsToSend) HandleSlotChange(ItemBase item, string slotId)
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

        private IEnumerator ResetEquipmentUi()
        {
            yield return new WaitForSeconds(0.1f);

            var equipmentUi = GameManager.Instance.UserInterface.GetCharacterMenuUiEquipmentTab();

            if (equipmentUi.gameObject.activeSelf)
            {
                equipmentUi.ResetEquipmentUi(true);
            }
        }

        public IEnumerable<ItemBase> GetHandItems()
        {
            return _items
                .Where(x => x.Value is Weapon or Consumer)
                .Select(x => x.Value)
                .OrderBy(x => x.Name);
        }

        public IEnumerable<ItemBase> GetCompatibleItems(string slotId)
        {
            if (slotId == null)
            {
                return _items
                    .Select(x => x.Value)
                    .OrderBy(x => x.Name);
            }

            var matches = _items
                .Where(x => !x.Value.RegistryTypeId.IsNullOrWhiteSpace() && slotId.StartsWith(x.Value.RegistryTypeId));

            if (!matches.Any())
            {
                matches = _items.Where(
                    i => i.Value is SpecialGear specialGear
                         && slotId.StartsWith(((ISpecialGear)specialGear.RegistryType).SlotId.ToString()));
            }

            return matches
                .Select(x => x.Value)
                .OrderBy(x => x.Name);
        }

        public void ApplyInventoryChanges(InventoryChanges changes)
        {
            if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
            {
                foreach (var id in changes.IdsToRemove)
                {
                    _items.Remove(id);
                }
                _playerState.AlertOfInventoryRemovals(changes.IdsToRemove.Length);
            }

            if (IsInventoryFull())
            {
                _playerState.AlertInventoryIsFull();

                //todo: zzz v0.7 - send to storage when inventory full

                return;
            }

            var itemsToAdd = Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(changes.Loot)
                .UnionIfNotNull(changes.Accessories)
                .UnionIfNotNull(changes.Armor)
                .UnionIfNotNull(changes.Consumers)
                .UnionIfNotNull(changes.ItemStacks)
                .UnionIfNotNull(changes.SpecialGear)
                .UnionIfNotNull(changes.Weapons);

            foreach (var item in itemsToAdd)
            {
                FillTypesFromIds(item);

                if (item is ItemStack itemStack)
                {
                    MergeItemStacks(itemStack);
                }
                else
                {
                    _items.Add(item.Id, item);
                }
            }

            var itemToAddCount = itemsToAdd.Count();

            switch (itemToAddCount)
            {
                case 1:
                    var alert1Text = _localizer.Translate("ui.alert.itemadded");
                    _playerState.ShowAlertForItemsAddedToInventory(string.Format(alert1Text, itemsToAdd.First().Name));
                    break;

                default:
                    var alert2Text = _localizer.Translate("ui.alert.itemsadded");
                    _playerState.ShowAlertForItemsAddedToInventory(string.Format(alert2Text, itemToAddCount));
                    break;
            }

            GameManager.Instance.QueueAsapSave(_playerState.Username);
        }

        public void LoadInventory(InventoryData inventoryData)
        {
            _maxItemCount = inventoryData.MaxItems > 0
                ? inventoryData.MaxItems
                : 30;

            var itemsToAdd = Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(inventoryData.Loot)
                .UnionIfNotNull(inventoryData.Accessories)
                .UnionIfNotNull(inventoryData.Armor)
                .UnionIfNotNull(inventoryData.Weapons)
                .UnionIfNotNull(inventoryData.Consumers)
                .UnionIfNotNull(inventoryData.ItemStacks)
                .UnionIfNotNull(inventoryData.SpecialGear);

            foreach (var item in itemsToAdd)
            {
                FillTypesFromIds(item);
                _items.Add(item.Id, item);
            }

            if (inventoryData.EquippedItems != null)
            {
                foreach (var kvp in inventoryData.EquippedItems)
                {
                    if (kvp.Value.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    var slotId = kvp.Key;

                    var item = itemsToAdd.First(x => x.Id == kvp.Value);

                    TriggerSlotChangeEvent(item, slotId);

                    SpawnEquippedObject(item, slotId);
                }
            }

            if (inventoryData.ShapeMapping != null)
            {
                foreach (var kvp in inventoryData.ShapeMapping)
                {
                    _itemIdToShapeMapping.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private void TriggerSlotChangeEvent(ItemBase item, string slotId)
        {
            var eventArgs = new SlotChangeEventArgs(this, slotId, item?.Id);
            _eventManager.Trigger(EventIdSlotChange, eventArgs);
            _eventManager.After(EventIdSlotChange, eventArgs);
        }

        protected override void SetEquippedItem(string itemId, string slotId)
        {
            var item = itemId.IsNullOrWhiteSpace() ? null : _items[itemId];

            if (_equippedItems.TryGetValue(slotId, out var equippedItem))
            {
                equippedItem.Item = item;
            }
            else
            {
                if (!IsValidSlotId(slotId))
                {
                    Debug.LogWarning($"Invalid slot ID {slotId}");
                    return;
                }

                _equippedItems.Add(slotId, new EquippedItem
                {
                    Item = item
                });
            }

            if (slotId == HandSlotIds.LeftHand)
            {
                _playerState.HandStatusLeft.SetEquippedItem(item, item?.GetDescription(_localizer));
            }
            else if (slotId == HandSlotIds.RightHand)
            {
                _playerState.HandStatusRight.SetEquippedItem(item, item?.GetDescription(_localizer));
            }
        }

        public InventoryData GetSaveData()
        {
            var groupedItems = _items
                .Select(x => x.Value)
                .GroupBy(x => x.GetType());

            var equippedItems = _equippedItems
                .Where(x => !(x.Value?.Item?.Id.IsNullOrWhiteSpace() ?? false))
                .Select(x => new Api.Utilities.Data.KeyValuePair<string, string>(x.Key, x.Value.Item?.Id));

            var shapeMapping = _itemIdToShapeMapping
                .Select(x => new Api.Utilities.Data.KeyValuePair<string, string>(x.Key, x.Value));

            return new InventoryData
            {
                MaxItems = _maxItemCount,
                Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(Loot))?.Select(x => x as Loot).ToArray(),
                Accessories = groupedItems.FirstOrDefault(x => x.Key == typeof(Accessory))?.Select(x => x as Accessory).ToArray(),
                Armor = groupedItems.FirstOrDefault(x => x.Key == typeof(Armor))?.Select(x => x as Armor).ToArray(),
                Consumers = groupedItems.FirstOrDefault(x => x.Key == typeof(Consumer))?.Select(x => x as Consumer).ToArray(),
                Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray(),
                ItemStacks = groupedItems.FirstOrDefault(x => x.Key == typeof(ItemStack))?.Select(x => x as ItemStack).ToArray(),
                SpecialGear = groupedItems.FirstOrDefault(x => x.Key == typeof(SpecialGear))?.Select(x => x as SpecialGear).ToArray(),
                EquippedItems = equippedItems.ToArray(),
                ShapeMapping = shapeMapping.ToArray()
            };
        }

        public KeyValuePair<string, EquippedItem>? GetEquippedWithItemId(string itemId)
        {
            var match = _equippedItems.FirstOrDefault(x => x.Value?.Item?.Id == itemId);
            return match.Value == null ? null : match;
        }

        private void DespawnEquippedObject(string slotId)
        {
            if (!_equippedItems.ContainsKey(slotId))
            {
                return;
            }

            var currentlyInGame = _equippedItems[slotId].GameObject;

            if (currentlyInGame == null)
            {
                return;
            }

            currentlyInGame.name = "DESTROY" + currentlyInGame.name;
            Destroy(currentlyInGame);
            _equippedItems[slotId].GameObject = null;
        }

        private void SpawnEquippedObject(ItemBase item, string slotId)
        {
            DespawnEquippedObject(slotId);

            var isLeftHand = slotId == HandSlotIds.LeftHand;

            if (item == null)
            {
                return;
            }

            //todo: zzz v0.4.1 - handle custom visuals for equipped items

            switch (slotId)
            {
                case HandSlotIds.LeftHand:
                case HandSlotIds.RightHand:
                    SpawnItemInHand(slotId, item, isLeftHand);
                    break;

                    //case SlotGameObjectName.Amulet:
                    //    InstantiateAccessory(slotGameObjectName, item, _playerState.GraphicsTransform, manipulateTransform: t => t.position += t.forward * _amuletForwardMultiplier);
                    //    break;

                    //case SlotGameObjectName.Belt:
                    //    InstantiateAccessory(slotGameObjectName, item, _playerState.GraphicsTransform);
                    //    break;

                    //case SlotGameObjectName.LeftRing:
                    //    InstantiateAccessory(slotGameObjectName, item, _playerState.BodyParts.LeftArm, true);
                    //    break;

                    //case SlotGameObjectName.RightRing:
                    //    InstantiateAccessory(slotGameObjectName, item, _playerState.BodyParts.RightArm, true);
                    //    break;

                    //case SlotGameObjectName.Helm:
                    //    InstantiateArmor(slotGameObjectName, item, _playerState.BodyParts.Head);
                    //    break;

                    //case SlotGameObjectName.Barrier:
                    //case SlotGameObjectName.Chest:
                    //case SlotGameObjectName.Legs:
                    //case SlotGameObjectName.Feet:
                    //    InstantiateArmor(slotGameObjectName, item, _playerState.GraphicsTransform);
                    //    break;

                    //default:
                    //    Debug.LogWarning("Not yet implemented equipping for slot " + slotId);
                    //    break;
            }
        }

        private void SpawnItemInHand(string slotId, ItemBase item, bool isLeftHand = true)
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                Debug.LogError("Tried to spawn a GameObject on a server");
                return;
            }

            switch (item)
            {
                case Weapon weapon:
                    _typeRegistry.LoadAddessable<GameObject>(
                       weapon.Visuals.PrefabAddress,
                       prefab =>
                       {
                           InstantiateInPlayerHand(prefab, isLeftHand, new Vector3(0, 90), slotId);
                       });

                    break;

                case Consumer consumer:
                    _typeRegistry.LoadAddessable<GameObject>(
                        consumer.ResourceType.ItemInHandDefaultPrefab,
                        prefab =>
                        {
                            if (prefab != null)
                            {
                                InstantiateInPlayerHand(prefab, isLeftHand, null, slotId);
                            }
                        });

                    break;

                default:
                    Debug.LogWarning($"Not implemented SpawnItemInHand handling for item type {item.GetType().Name}");
                    break;
            }
        }

        private void InstantiateInPlayerHand(GameObject prefab, bool isLeftHand, Vector3? rotation, string slotId)
        {
            var newObj = Instantiate(prefab, _playerState.InFrontOfPlayer.transform);

            newObj.transform.localPosition = isLeftHand
                ? _playerState.Positions.LeftHand.localPosition
                : _playerState.Positions.RightHand.localPosition;

            if (rotation.HasValue)
            {
                newObj.transform.localEulerAngles = rotation.Value;
            }

            if (IsOwner)
            {
                newObj.SetGameLayerRecursive(_playerState.InFrontOfPlayer.layer);
            }

            _equippedItems[slotId].GameObject = newObj;
        }

        //todo: zzz v0.4.1 - InstantiateAccessory 
        //private void InstantiateAccessory(
        //    string slotId,
        //    ItemBase item,
        //    Transform parentTransform,
        //    bool showsOnPlayerCamera = false,
        //    Action<Transform> manipulateTransform = null)
        //{
        //    var thisClient = NetworkManager.LocalClientId == OwnerClientId;

        //    if (!showsOnPlayerCamera && thisClient)
        //    {
        //        return;
        //    }

        //    if (item is not Accessory accessoryItem)
        //    {
        //        Debug.LogError("Item is not an accessory");
        //        return;
        //    }

        //    _typeRegistry.LoadAddessable(
        //        accessoryItem.Visuals.PrefabAddress,
        //        prefab =>
        //        {
        //            var newObj = Instantiate(prefab, parentTransform);

        //            manipulateTransform?.Invoke(newObj.transform);

        //            if (showsOnPlayerCamera && thisClient)
        //            {
        //                newObj.SetGameLayerRecursive(LayerMask.NameToLayer(Layers.InFrontOfPlayer));
        //            }

        //            _equippedItems[slotId].GameObject = newObj;
        //        });
        //}

        //todo: zzz v0.4.1 - InstantiateArmor
        //private void InstantiateArmor(
        //    string slotId,
        //    ItemBase item,
        //    Transform parentTransform)
        //{
        //    if (item is not Armor armorItem)
        //    {
        //        Debug.LogError("Item is not armor");
        //        return;
        //    }

        //    if (NetworkManager.LocalClientId == OwnerClientId)
        //    {
        //        return;
        //    }

        //    _typeRegistry.LoadAddessable(
        //        armorItem.Visuals.PrefabAddress,
        //        prefab =>
        //        {
        //            var newObj = Instantiate(prefab, parentTransform);
        //            _equippedItems[slotId].GameObject = newObj;
        //        });
        //}

        public void AddItemAsAdmin(ItemBase item)
        {
            GameManager.Instance.CheckIsAdmin();

            FillTypesFromIds(item);
            _items.Add(item.Id, item);
        }

        public string GetAssignedShape(string itemId)
        {
            if (!_itemIdToShapeMapping.ContainsKey(itemId))
            {
                return null;
            }

            return _itemIdToShapeMapping[itemId];
        }

        public bool SetAssignedShape(string itemId, string shape)
        {
            if (shape.IsNullOrWhiteSpace())
            {
                _itemIdToShapeMapping.Remove(itemId);
                return true;
            }

            var conflict = _itemIdToShapeMapping.Any(x => x.Key != itemId && x.Value == shape);
            if (conflict)
            {
                return false;
            }

            _itemIdToShapeMapping[itemId] = shape;
            return true;
        }

        public ItemBase GetItemFromAssignedShape(string shapeCode)
        {
            var comparisonShapeCode = GetShapeCodeWithoutLengths(shapeCode);

            var match = _itemIdToShapeMapping
                .FirstOrDefault(x => GetShapeCodeWithoutLengths(x.Value) == comparisonShapeCode);

            if (match.Key.IsNullOrWhiteSpace())
            {
                return null;
            }

            return GetItemWithId<ItemBase>(match.Key);
        }

        private string GetShapeCodeWithoutLengths(string shapeCode)
        {
            return Regex.Replace(shapeCode, "(:\\d+)", string.Empty);
        }
    }
}
