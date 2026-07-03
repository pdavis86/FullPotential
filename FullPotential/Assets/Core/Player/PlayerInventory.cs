using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete.Items.Base;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Models.Player;

using Unity.Netcode;

using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Player
{
    // todo: zzz v0.6 - aim to make this class redundant
    public class PlayerInventory : InventoryBase, IPlayerInventory
    {
        private Dictionary<string, CombatItemBase> _combatItemsWithShape;
        private PlayerFighter _playerFighter;

        #region Unity Events Handlers

        // ReSharper disable once UnusedMember.Local
        protected override void Awake()
        {
            base.Awake();

            _playerFighter = GetComponent<PlayerFighter>();
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void EquipItemServerRpc(string itemId, string slotId)
        {
            EquipItemAsync(itemId, slotId).Forget();
        }

        #endregion

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void EquipItemClientRpc(string itemId, string slotId, ClientRpcParams clientRpcParams)
        {
            EquipItemAsync(itemId, slotId).Forget();
        }

        private async UniTask EquipItemAsync(string itemId, string slotId)
        {
            ItemBase item;
            if (!_items.ContainsKey(itemId))
            {
                var itemData = (await _dataLoader.GetInventoryItemDataAsync(_characterId, new[] { itemId }))[0];
                item = _itemFactory.GetItemFromData(itemData);
                _items.Add(item.Id, item);
            }
            else
            {
                item = _items[itemId];
            }

            HandleSlotChange(item, slotId);

            if (IsServer)
            {
                MarkAsDirtyAndAddToQueue();

                var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, 0);
                EquipItemClientRpc(itemId, slotId, nearbyClients);
            }
        }

        // todo: zzz v0.6 - This should be an event
        private async UniTask ResetEquipmentUiAsync()
        {
            await UniTask.WaitForSeconds(0.1f);

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
                         && slotId.StartsWith(((ISpecialGearType)specialGear.RegistryType).SlotIdString));
            }

            return matches
                .Select(x => x.Value)
                .OrderBy(x => x.Name);
        }

        public override void LoadInventory(InventoryData inventoryData)
        {
            base.LoadInventory(inventoryData);

            _combatItemsWithShape = _items
                .Where(x => x.Value is CombatItemBase combatItem && combatItem.ShapeCode != null)
                .ToDictionary(x => x.Key, x => (CombatItemBase)x.Value);
        }

        protected override void ApplyEquippedItemChange(string itemId, string slotId)
        {
            var changes = new Dictionary<string, string> { { slotId, itemId } };
            ApplyEquippedItemChanges(changes);
        }

        protected override void ApplyEquippedItemChanges(Dictionary<string, string> equippedItems)
        {
            if (equippedItems == null || !equippedItems.Any())
            {
                return;
            }

            foreach (var kvp in equippedItems)
            {
                var item = kvp.Value.IsNullOrWhiteSpace() ? null : _items[kvp.Value];
                var slotId = kvp.Key;

                _playerFighter.GetSlotStatus(slotId)?.StopActiveConsumerBehaviour();

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

                    _equippedItems.Add(slotId, new EquippedItem { Item = item });
                }

                SpawnEquippedObject(item, slotId);
            }

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                ResetEquipmentUiAsync().Forget();
            }
            //else if (!IsServer)
            //{
            //    var keysToRemove = new List<string>();
            //    foreach (var kvp in _items)
            //    {
            //        if (GetEquippedWithItemId(kvp.Key) == null)
            //        {
            //            keysToRemove.Add(kvp.Key);
            //        }
            //    }
            //    foreach (var key in keysToRemove)
            //    {
            //        _items.Remove(key);
            //    }
            //}

            MarkAsDirtyAndAddToQueue();

            _playerFighter.UpdateUiHealthAndDefenceValues();
        }

        protected override void NotifyOfItemsAdded(IEnumerable<ItemBase> itemsAdded)
        {
            var itemsAddedCount = itemsAdded.Count();

            switch (itemsAddedCount)
            {
                case 0:
                    return;

                case 1:
                    var alert1Text = _localizer.Translate("ui.alert.itemadded");
                    _playerFighter.ShowAlertForItemsAddedToInventory(string.Format(alert1Text, itemsAdded.First().GetName(_localizer)));
                    break;

                default:
                    var alert2Text = _localizer.Translate("ui.alert.itemsadded");
                    _playerFighter.ShowAlertForItemsAddedToInventory(string.Format(alert2Text, itemsAddedCount));
                    break;
            }
        }

        protected override void NotifyOfInventoryFull()
        {
            _playerFighter.AlertInventoryIsFull();

            //todo: zzz v0.7 - send to storage when inventory full
        }

        protected override void NotifyOfItemsRemoved(IEnumerable<ItemBase> itemsRemoved)
        {
            var countRemoved = itemsRemoved.Count(x => x is not ItemStackBase);

            if (countRemoved == 0)
            {
                return;
            }

            _playerFighter.AlertOfInventoryRemovals(countRemoved);

            var craftingUi = GameManager.Instance.UserInterface.GetCharacterMenuUiCraftingTab();
            if (craftingUi.gameObject.activeSelf)
            {
                craftingUi.ResetUi();
            }
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

            if (item == null)
            {
                return;
            }

            if (item is SpecialGear)
            {
                InstantiateCustomGearVisuals(slotId, item);
                return;
            }

            if (item is Accessory)
            {
                InstantiateAccessoryVisuals(slotId, item);
                return;
            }

            if (item is Armor)
            {
                InstantiateArmorVisuals(slotId, item);
                return;
            }

            switch (slotId)
            {
                case HandSlotIds.LeftHand:
                case HandSlotIds.RightHand:
                    SpawnItemInHand(slotId, item);
                    break;

                default:
                    Debug.LogWarning("Not yet implemented equipping for slot " + slotId);
                    break;
            }
        }

        private void SpawnItemInHand(string slotId, ItemBase item)
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
                           InstantiateInPlayerHand(slotId, prefab, new Vector3(0, 90));
                       });

                    break;

                case Consumer consumer:
                    if (consumer.ResourceType.ItemInHandDefaultPrefab == null)
                    {
                        Debug.LogWarning($"No default prefab exists for resource type '{_localizer.Translate(consumer.ResourceType)}'");
                        return;
                    }

                    _typeRegistry.LoadAddessable<GameObject>(
                        consumer.ResourceType.ItemInHandDefaultPrefab,
                        prefab =>
                        {
                            if (prefab != null)
                            {
                                InstantiateInPlayerHand(slotId, prefab, null);
                            }
                        });

                    break;

                default:
                    Debug.LogWarning($"Not implemented SpawnItemInHand handling for item type {item.GetType().Name}");
                    break;
            }
        }

        private void InstantiateInPlayerHand(string slotId, GameObject prefab, Vector3? rotation)
        {
            var newObj = Instantiate(prefab, _playerFighter.InFrontOfPlayer.transform);

            newObj.transform.localPosition = slotId == HandSlotIds.LeftHand
                ? _playerFighter.Positions.LeftHand.localPosition
                : _playerFighter.Positions.RightHand.localPosition;

            if (rotation.HasValue)
            {
                newObj.transform.localEulerAngles = rotation.Value;
            }

            if (IsOwner)
            {
                newObj.SetGameLayerRecursive(_playerFighter.InFrontOfPlayer.layer);
            }

            _equippedItems[slotId].GameObject = newObj;
        }

        private void InstantiateAccessoryVisuals(
            string slotId,
            ItemBase item)
        {
            Transform parentTransform = null;
            var showsOnPlayerCamera = false;
            Action<Transform> manipulateTransform = null;

            //todo: zzz v0.6 - remove special cases
            switch (slotId)
            {
                case "ddeafb61-0163-4888-b355-16a37d3a33b5" + ";1": //SlotGameObjectName.Amulet:
                    const float amuletForwardMultiplier = 0.2f;
                    parentTransform = _playerFighter.GraphicsTransform;
                    manipulateTransform = t => t.position += t.forward * amuletForwardMultiplier;
                    break;

                case "6d4bce60-dda6-4a88-82fd-c2b086065c8b" + ";1": //SlotGameObjectName.Belt:
                    parentTransform = _playerFighter.GraphicsTransform;
                    break;

                case "b74b00f9-9cf1-4758-9e22-b4fbd4d1cea0" + ";1": //SlotGameObjectName.LeftRing:
                    parentTransform = _playerFighter.BodyParts.LeftArm;
                    showsOnPlayerCamera = true;
                    break;

                case "b74b00f9-9cf1-4758-9e22-b4fbd4d1cea0" + ";2": //SlotGameObjectName.RightRing:
                    parentTransform = _playerFighter.BodyParts.RightArm;
                    showsOnPlayerCamera = true;
                    break;
            }

            var thisClient = NetworkManager.LocalClientId == OwnerClientId;

            if (!showsOnPlayerCamera && thisClient)
            {
                return;
            }

            if (item is not Accessory accessoryItem)
            {
                Debug.LogError("Item is not an accessory");
                return;
            }

            if (accessoryItem.Visuals == null)
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                accessoryItem.Visuals.PrefabAddress,
                prefab =>
                {
                    var newObj = Instantiate(prefab, parentTransform);

                    manipulateTransform?.Invoke(newObj.transform);

                    if (showsOnPlayerCamera && thisClient)
                    {
                        newObj.SetGameLayerRecursive(LayerMask.NameToLayer(Layers.InFrontOfPlayer));
                    }

                    _equippedItems[slotId].GameObject = newObj;
                });
        }

        private void InstantiateArmorVisuals(
            string slotId,
            ItemBase item)
        {
            Transform parentTransform = null;

            //todo: zzz v0.6 - remove special cases
            switch (slotId)
            {
                case ArmorTypeIds.HelmId:
                    parentTransform = _playerFighter.BodyParts.Head;
                    break;

                case ArmorTypeIds.ChestId:
                case ArmorTypeIds.LegsId:
                case ArmorTypeIds.FeetId:
                    parentTransform = _playerFighter.GraphicsTransform;
                    break;
            }

            if (item is not Armor armorItem)
            {
                Debug.LogError("Item is not armor");
                return;
            }

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                return;
            }

            if (armorItem.Visuals == null)
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                armorItem.Visuals.PrefabAddress,
                prefab =>
                {
                    var newObj = Instantiate(prefab, parentTransform);
                    _equippedItems[slotId].GameObject = newObj;
                });
        }

        private void InstantiateCustomGearVisuals(
            string slotId,
            ItemBase item)
        {
            if (item is not SpecialGear specialGearItem)
            {
                Debug.LogError("Item is not special gear");
                return;
            }

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                return;
            }

            if (specialGearItem.Visuals == null)
            {
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                specialGearItem.Visuals.PrefabAddress,
                prefab =>
                {
                    var newObj = Instantiate(prefab, _playerFighter.GraphicsTransform);
                    _equippedItems[slotId].GameObject = newObj;
                });
        }

        public void AddItemAsAdmin(ItemBase item)
        {
            GameManager.Instance.CheckIsAdmin();

            _itemFactory.FillTypesFromIds(item);
            _items.Add(item.Id, item);

            MarkAsDirtyAndAddToQueue();
        }

        public string GetAssignedShape(string itemId)
        {
            if (!_combatItemsWithShape.ContainsKey(itemId))
            {
                return null;
            }

            return _combatItemsWithShape[itemId].ShapeCode;
        }

        public bool SetAssignedShape(string itemId, string shapeCode)
        {
            if (shapeCode.IsNullOrWhiteSpace())
            {
                _combatItemsWithShape[itemId].ShapeCode = null;
                _combatItemsWithShape[itemId].IsDirty = true;
                _combatItemsWithShape.Remove(itemId);
                return true;
            }

            var conflict = _combatItemsWithShape.Any(x => x.Key != itemId && x.Value.ShapeCode == shapeCode);
            if (conflict)
            {
                return false;
            }

            if (!_combatItemsWithShape.ContainsKey(itemId))
            {
                _combatItemsWithShape.Add(itemId, (CombatItemBase)_items[itemId]);
            }

            _combatItemsWithShape[itemId].ShapeCode = shapeCode;
            _combatItemsWithShape[itemId].IsDirty = true;
            return true;
        }

        public ItemBase GetItemFromAssignedShape(string shapeCode)
        {
            var comparisonShapeCode = GetShapeCodeWithoutLengths(shapeCode);

            var match = _combatItemsWithShape
                .FirstOrDefault(x => GetShapeCodeWithoutLengths(x.Value.ShapeCode) == comparisonShapeCode);

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
