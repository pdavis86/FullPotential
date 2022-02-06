using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Combat;
using FullPotential.Api.Constants;
using FullPotential.Api.Data;
using FullPotential.Api.Enums;
using FullPotential.Api.Extensions;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Helpers;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Loot;
using FullPotential.Api.Registry.Spells;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Data;
using FullPotential.Core.Extensions;
using FullPotential.Core.Networking;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.PlayerBehaviours
{
    public class PlayerInventory : NetworkBehaviour, IPlayerInventory
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        [SerializeField] private float _amuletForwardMultiplier = 0.2f;

        private PlayerState _playerState;
        private Dictionary<string, ItemBase> _items;
        private Dictionary<SlotGameObjectName, EquippedItem> _equippedItems;
        private int _armorSlotCount;
        private int _maxItems;

        private readonly FragmentedMessageReconstructor _inventoryChangesReconstructor = new FragmentedMessageReconstructor();

        #region Event Handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerState = GetComponent<PlayerState>();

            _items = new Dictionary<string, ItemBase>();
            _equippedItems = new Dictionary<SlotGameObjectName, EquippedItem>();

            _armorSlotCount = Enum.GetNames(typeof(IGearArmor.ArmorCategory)).Length;
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void EquipItemServerRpc(string itemId, SlotGameObjectName slotGameObjectName)
        {
            var slotsToSend = new List<string> { slotGameObjectName.ToString() };

            var previousKvp = _equippedItems
                .FirstOrDefault(x => x.Value.Item != null && x.Value?.Item.Id == itemId);

            var previouslyInSlot = previousKvp.Value != null ? (SlotGameObjectName?)previousKvp.Key : null;

            var item = _items[itemId];

            if (!string.IsNullOrWhiteSpace(itemId) && previouslyInSlot.HasValue)
            {
                if (previouslyInSlot.Value != slotGameObjectName)
                {
                    slotsToSend.Add(previouslyInSlot.Value.ToString());
                }

                _equippedItems[previouslyInSlot.Value].Item = null;
            }

            var wasEquipped = false;
            if (!previouslyInSlot.HasValue || previouslyInSlot.Value != slotGameObjectName)
            {
                SetEquippedItem(item, slotGameObjectName);
                wasEquipped = true;
            }

            if (slotGameObjectName == SlotGameObjectName.LeftHand || slotGameObjectName == SlotGameObjectName.RightHand)
            {
                var otherHandSlot = slotGameObjectName == SlotGameObjectName.LeftHand
                    ? SlotGameObjectName.RightHand
                    : SlotGameObjectName.LeftHand;

                if (item is Weapon weapon && weapon.IsTwoHanded)
                {
                    SetEquippedItem(null, otherHandSlot);
                    slotsToSend.Add(otherHandSlot.ToString());
                }
                else
                {
                    var itemInOtherHand = GetItemInSlot(otherHandSlot);
                    if (itemInOtherHand is Weapon otherWeapon && otherWeapon.IsTwoHanded)
                    {
                        SetEquippedItem(null, otherHandSlot);
                        slotsToSend.Add(otherHandSlot.ToString());
                    }
                }
            }

            var saveData = GameManager.Instance.UserRegistry.PlayerData[_playerState.Username];
            saveData.Inventory = GetSaveData();
            saveData.IsDirty = true;

            var invChange = new InventoryChanges
            {
                EquippedItems = saveData.Inventory.EquippedItems.Where(x => slotsToSend.Contains(x.Key)).ToArray()
            };

            if (wasEquipped)
            {
                InventoryDataHelper.PopulateInventoryChangesWithItem(invChange, item);
            }

            foreach (var message in FragmentedMessageReconstructor.GetFragmentedMessages(invChange))
            {
                ApplyEquipChangeClientRpc(message, GameManager.Instance.RpcHelper.ForNearbyPlayers());
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
                .UnionIfNotNull(changes.Spells)
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
                var slotGameObjectName = Enum.Parse<SlotGameObjectName>(sourceKvp.Key);

                SetEquippedItem(item, slotGameObjectName);
                SpawnEquippedObject(item, slotGameObjectName);
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

            _playerState.UpdateHealthAndDefenceValues();
        }

        #endregion

        private IEnumerator ResetEquipmentUi()
        {
            yield return new WaitForSeconds(0.1f);

            var equipmentUi = GameManager.Instance.MainCanvasObjects.GetCharacterMenuUiEquipmentTab();

            if (equipmentUi.gameObject.activeSelf)
            {
                equipmentUi.ResetEquipmentUi(true);
            }
        }

        public IEnumerable<ItemBase> GetCompatibleItemsForSlot(IGear.GearCategory? gearCategory)
        {
            if (gearCategory == null)
            {
                return _items.Select(x => x.Value);
            }

            if (gearCategory == IGear.GearCategory.Hand)
            {
                return _items
                    .Where(x => x.Value is Weapon or Spell)
                    .Select(x => x.Value);
            }

            return _items
                .Where(x =>
                    (x.Value is Accessory acc && (int)((IGearAccessory)acc.RegistryType).Category == (int)gearCategory)
                    || (x.Value is Armor armor && (int)((IGearArmor)armor.RegistryType).Category == (int)gearCategory))
                .Select(x => x.Value);
        }

        public int GetDefenseValue()
        {
            var defenseSum = 0;

            foreach (SlotGameObjectName slotGameObjectName in Enum.GetValues(typeof(SlotGameObjectName)))
            {
                var equippedItemId = _equippedItems.ContainsKey(slotGameObjectName)
                    ? _equippedItems[slotGameObjectName].Item?.Id
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

            if (_items.Count >= _maxItems)
            {
                _playerState.AlertInventoryIsFull();
                return;
            }

            var itemsToAdd = Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(changes.Loot)
                .UnionIfNotNull(changes.Accessories)
                .UnionIfNotNull(changes.Armor)
                .UnionIfNotNull(changes.Spells)
                .UnionIfNotNull(changes.Weapons);

            foreach (var item in itemsToAdd)
            {
                FillTypesFromIds(item);
                _items.Add(item.Id, item);
            }

            var itemToAddCount = itemsToAdd.Count();

            switch (itemToAddCount)
            {
                case 1:
                    var alert1Text = GameManager.Instance.Localizer.Translate("ui.alert.itemadded");
                    _playerState.ShowAlertForItemsAddedToInventory(string.Format(alert1Text, itemsToAdd.First().Name));
                    break;

                default:
                    var alert2Text = GameManager.Instance.Localizer.Translate("ui.alert.itemsadded");
                    _playerState.ShowAlertForItemsAddedToInventory(string.Format(alert2Text, itemToAddCount));
                    break;
            }

            if (IsServer)
            {
                var saveData = GameManager.Instance.UserRegistry.PlayerData[_playerState.Username];
                saveData.Inventory = GetSaveData();
                saveData.IsDirty = true;
            }
        }

        public void LoadInventory(InventoryData inventoryData)
        {
            _maxItems = inventoryData.MaxItems > 0
                ? inventoryData.MaxItems
                : 30;

            var itemsToAdd = Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(inventoryData.Loot)
                .UnionIfNotNull(inventoryData.Accessories)
                .UnionIfNotNull(inventoryData.Armor)
                .UnionIfNotNull(inventoryData.Spells)
                .UnionIfNotNull(inventoryData.Weapons);

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

                    if (!Enum.TryParse<SlotGameObjectName>(kvp.Key, out var slotGameObjectName))
                    {
                        Debug.LogError($"Failed to load slot data for {kvp.Key}");
                    }

                    var item = itemsToAdd.First(x => x.Id == kvp.Value);

                    SetEquippedItem(item, slotGameObjectName);

                    SpawnEquippedObject(item, slotGameObjectName);
                }
            }
        }

        private void SetEquippedItem(ItemBase item, SlotGameObjectName slotGameObjectName)
        {
            if (_equippedItems.ContainsKey(slotGameObjectName))
            {
                _equippedItems[slotGameObjectName].Item = item;
            }
            else
            {
                _equippedItems.Add(slotGameObjectName, new EquippedItem
                {
                    Item = item
                });
            }
        }

        private static void FillTypesFromIds(ItemBase item)
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

        private InventoryData GetSaveData()
        {
            var equippedItems = _equippedItems
                .Where(x => !(x.Value?.Item?.Id.IsNullOrWhiteSpace() ?? false))
                .Select(x => new Api.Data.KeyValuePair<string, string>(
                    Enum.GetName(typeof(SlotGameObjectName), x.Key),
                    x.Value.Item?.Id));

            var groupedItems = _items
                .Select(x => x.Value)
                .GroupBy(x => x.GetType());

            return new InventoryData
            {
                MaxItems = _maxItems,
                Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(Loot))?.Select(x => x as Loot).ToArray(),
                Accessories = groupedItems.FirstOrDefault(x => x.Key == typeof(Accessory))?.Select(x => x as Accessory).ToArray(),
                Armor = groupedItems.FirstOrDefault(x => x.Key == typeof(Armor))?.Select(x => x as Armor).ToArray(),
                Spells = groupedItems.FirstOrDefault(x => x.Key == typeof(Spell))?.Select(x => x as Spell).ToArray(),
                Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray(),
                EquippedItems = equippedItems.ToArray()
            };
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
            return _equippedItems.ContainsKey(slotGameObjectName)
                ? _equippedItems[slotGameObjectName].Item
                : null;
        }

        public System.Collections.Generic.KeyValuePair<SlotGameObjectName, EquippedItem>? GetEquippedWithItemId(string itemId)
        {
            var match = _equippedItems.FirstOrDefault(x => x.Value?.Item?.Id == itemId);
            return match.Value == null ? null : match;
        }

        public List<ItemBase> GetComponentsFromIds(string[] componentIds)
        {
            //Check that the components are actually in the player's inventory and load them in the order they are given
            var components = new List<ItemBase>();
            foreach (var id in componentIds)
            {
                var match = GetItemWithId<ItemBase>(id);
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
                return new List<string> { GameManager.Instance.Localizer.Translate("crafting.error.nocomponents") };
            }

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

        private void DespawnEquippedObject(SlotGameObjectName slotGameObjectName)
        {
            if (!_equippedItems.ContainsKey(slotGameObjectName))
            {
                return;
            }

            var currentlyInGame = _equippedItems[slotGameObjectName].GameObject;

            if (currentlyInGame == null)
            {
                return;
            }

            currentlyInGame.name = "DESTROY" + currentlyInGame.name;
            Destroy(currentlyInGame);
            _equippedItems[slotGameObjectName].GameObject = null;
        }

        private void SpawnEquippedObject(ItemBase item, SlotGameObjectName slotGameObjectName)
        {
            DespawnEquippedObject(slotGameObjectName);

            if (item == null)
            {
                if (IsOwner && IsClient)
                {
                    switch (slotGameObjectName)
                    {
                        case SlotGameObjectName.LeftHand:
                        case SlotGameObjectName.RightHand:
                            GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateHand(null, slotGameObjectName == SlotGameObjectName.LeftHand);
                            break;
                    }
                }

                return;
            }

            switch (slotGameObjectName)
            {
                case SlotGameObjectName.LeftHand:
                case SlotGameObjectName.RightHand:
                    SpawnItemInHand(slotGameObjectName, item, slotGameObjectName == SlotGameObjectName.LeftHand);
                    break;

                case SlotGameObjectName.Amulet:
                    InstantiateAccessory(slotGameObjectName, item, _playerState.GraphicsTransform, manipulateTransform: t => t.position += t.forward * _amuletForwardMultiplier);
                    break;

                case SlotGameObjectName.Belt:
                    InstantiateAccessory(slotGameObjectName, item, _playerState.GraphicsTransform);
                    break;

                case SlotGameObjectName.LeftRing:
                    InstantiateAccessory(slotGameObjectName, item, _playerState.BodyParts.LeftArm, true);
                    break;

                case SlotGameObjectName.RightRing:
                    InstantiateAccessory(slotGameObjectName, item, _playerState.BodyParts.RightArm, true);
                    break;

                case SlotGameObjectName.Helm:
                    InstantiateArmor(slotGameObjectName, item, _playerState.BodyParts.Head);
                    break;

                case SlotGameObjectName.Barrier:
                case SlotGameObjectName.Chest:
                case SlotGameObjectName.Legs:
                case SlotGameObjectName.Feet:
                    InstantiateArmor(slotGameObjectName, item, _playerState.GraphicsTransform);
                    break;

                default:
                    Debug.LogWarning("Not yet implemented equipping for slot " + slotGameObjectName);
                    break;
            }
        }

        private void SpawnItemInHand(SlotGameObjectName slotGameObjectName, ItemBase item, bool isLeftHand = true)
        {
            if (IsOwner)
            {
                var contents = GameManager.Instance.ResultFactory.GetItemDescription(item);
                GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateHand(contents, isLeftHand);
            }

            if (!NetworkManager.Singleton.IsClient)
            {
                Debug.LogError("Tried to spawn a GameObject on a server");
                return;
            }

            switch (item)
            {
                case Weapon weapon:
                    if (item.RegistryType is not IGearWeapon weaponRegistryType)
                    {
                        Debug.LogError("Item is not a weapon");
                        return;
                    }

                    GameManager.Instance.TypeRegistry.LoadAddessable(
                        weapon.IsTwoHanded ? weaponRegistryType.PrefabAddressTwoHanded : weaponRegistryType.PrefabAddress,
                        prefab =>
                        {
                            InstantiateInPlayerHand(prefab, isLeftHand, new Vector3(0, 90), slotGameObjectName);
                        }
                    );

                    //todo: attribute-based ammo max
                    var newAmmoStatus = new PlayerHandStatus
                    {
                        AmmoMax = 5,
                        Ammo = 5
                    };

                    if (isLeftHand)
                    {
                        _playerState.HandStatusLeft = newAmmoStatus;
                    }
                    else
                    {
                        _playerState.HandStatusRight = newAmmoStatus;
                    }

                    GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateAmmo(isLeftHand, newAmmoStatus);

                    break;

                case Spell spell:
                    GameManager.Instance.TypeRegistry.LoadAddessable(
                        spell.Targeting.IdlePrefabAddress,
                        prefab =>
                        {
                            InstantiateInPlayerHand(prefab, isLeftHand, null, slotGameObjectName);
                        }
                    );

                    GameManager.Instance.MainCanvasObjects.HudOverlay.UpdateAmmo(isLeftHand, null);

                    break;

                default:
                    Debug.LogWarning($"Not implemented SpawnItemInHand handling for item type {item.GetType().Name}");
                    break;
            }
        }

        private void InstantiateInPlayerHand(GameObject prefab, bool isLeftHand, Vector3? rotation, SlotGameObjectName slotGameObjectName)
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
                GameObjectHelper.SetGameLayerRecursive(newObj, _playerState.InFrontOfPlayer.layer);
            }

            _equippedItems[slotGameObjectName].GameObject = newObj;
        }

        private void InstantiateAccessory(
            SlotGameObjectName slotGameObjectName,
            ItemBase item,
            Transform parentTransform,
            bool showsOnPlayerCamera = false,
            Action<Transform> manipulateTransform = null)
        {
            var thisClient = NetworkManager.LocalClientId == OwnerClientId;

            if (!showsOnPlayerCamera && thisClient)
            {
                return;
            }

            if (item.RegistryType is not IGearAccessory registryType)
            {
                Debug.LogError("Item is not an accessory");
                return;
            }

            GameManager.Instance.TypeRegistry.LoadAddessable(
                registryType.PrefabAddress,
                prefab =>
                {
                    var newObj = Instantiate(prefab, parentTransform);

                    manipulateTransform?.Invoke(newObj.transform);

                    if (showsOnPlayerCamera && thisClient)
                    {
                        GameObjectHelper.SetGameLayerRecursive(newObj, LayerMask.NameToLayer(Layers.InFrontOfPlayer));
                    }

                    _equippedItems[slotGameObjectName].GameObject = newObj;
                });
        }

        private void InstantiateArmor(
            SlotGameObjectName slotGameObjectName,
            ItemBase item,
            Transform parentTransform)
        {
            if (item.RegistryType is not IGearArmor registryType)
            {
                Debug.LogError("Item is not armor");
                return;
            }

            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                return;
            }

            GameManager.Instance.TypeRegistry.LoadAddessable(
                registryType.PrefabAddress,
                prefab =>
                {
                    var newObj = Instantiate(prefab, parentTransform);
                    _equippedItems[slotGameObjectName].GameObject = newObj;
                });
        }
    }
}
