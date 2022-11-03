using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.SpellsAndGadgets;
using FullPotential.Api.Items.Weapons;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Networking;
using FullPotential.Core.Networking.Data;
using FullPotential.Core.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.PlayerBehaviours
{
    public class PlayerInventory : InventoryBase, IPlayerInventory
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [SerializeField] private float _amuletForwardMultiplier = 0.2f;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        //Services
        private ITypeRegistry _typeRegistry;
        private IRpcService _rpcService;
        private ILocalizer _localizer;
        private IInventoryDataService _inventoryDataService;

        private PlayerState _playerState;
        private int _maxItems;

        private readonly FragmentedMessageReconstructor _inventoryChangesReconstructor = new FragmentedMessageReconstructor();

        #region Unity Events Handlers

        // ReSharper disable once UnusedMember.Local
        protected override void Awake()
        {
            base.Awake();

            _playerState = GetComponent<PlayerState>();

            _typeRegistry = GameManager.Instance.GetService<ITypeRegistry>();
            _rpcService = GameManager.Instance.GetService<IRpcService>();
            _localizer = GameManager.Instance.GetService<ILocalizer>();
            _inventoryDataService = GameManager.Instance.GetService<IInventoryDataService>();
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void EquipItemServerRpc(string itemId, SlotGameObjectName slotGameObjectName)
        {
            var item = _items[itemId];

            var slotChange = HandleSlotChange(item, slotGameObjectName);

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
                .UnionIfNotNull(changes.Gadgets)
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

            _playerState.UpdateUiHealthAndDefenceValues();
        }

        #endregion

        //todo: generalise for use in LivingEntityBase
        public (bool WasEquipped, List<string> SlotsToSend) HandleSlotChange(ItemBase item, SlotGameObjectName slotGameObjectName)
        {
            var slotsToSend = new List<string> { slotGameObjectName.ToString() };

            var previousKvp = _equippedItems
                .FirstOrDefault(x => x.Value.Item != null && x.Value?.Item.Id == item.Id);

            var previouslyInSlot = previousKvp.Value != null ? (SlotGameObjectName?)previousKvp.Key : null;

            if (previouslyInSlot.HasValue)
            {
                if (previouslyInSlot.Value != slotGameObjectName)
                {
                    slotsToSend.Add(previouslyInSlot.Value.ToString());
                }

                _equippedItems[previouslyInSlot.Value].Item = null;
                SetEquippedItem(null, slotGameObjectName);
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

        public IEnumerable<ItemBase> GetCompatibleItemsForSlot(IGear.GearCategory? gearCategory)
        {
            if (gearCategory == null)
            {
                return _items.Select(x => x.Value);
            }

            IEnumerable<KeyValuePair<string, ItemBase>> itemsForSlot;

            if (gearCategory == IGear.GearCategory.Hand)
            {
                itemsForSlot = _items
                    .Where(x => x.Value is Weapon or Spell or Gadget);
            }
            else
            {
                itemsForSlot = _items
                .Where(x =>
                    (x.Value is Accessory acc && (int)((IGearAccessory)acc.RegistryType).Category == (int)gearCategory)
                    || (x.Value is Armor armor && (int)((IGearArmor)armor.RegistryType).Category == (int)gearCategory));
            }

            return itemsForSlot
                .Select(x => x.Value)
                .OrderBy(x => x.Name);
        }

        public bool IsInventoryFull()
        {
            return _items.Count >= _maxItems;
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
                return;
            }

            var itemsToAdd = Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(changes.Loot)
                .UnionIfNotNull(changes.Accessories)
                .UnionIfNotNull(changes.Armor)
                .UnionIfNotNull(changes.Gadgets)
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
            _maxItems = inventoryData.MaxItems > 0
                ? inventoryData.MaxItems
                : 30;

            var itemsToAdd = Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(inventoryData.Loot)
                .UnionIfNotNull(inventoryData.Accessories)
                .UnionIfNotNull(inventoryData.Armor)
                .UnionIfNotNull(inventoryData.Gadgets)
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

            if (slotGameObjectName == SlotGameObjectName.LeftHand)
            {
                _playerState.HandStatusLeft.SetEquippedItem(item, item.GetDescription(_localizer));
            }
            else if (slotGameObjectName == SlotGameObjectName.RightHand)
            {
                _playerState.HandStatusRight.SetEquippedItem(item, item.GetDescription(_localizer));
            }
        }

        private void FillTypesFromIds(ItemBase item)
        {
            if (!string.IsNullOrWhiteSpace(item.RegistryTypeId) && item.RegistryType == null)
            {
                item.RegistryType = _typeRegistry.GetRegisteredForItem(item);
            }

            if (item is IHasTargetingAndShape magicalItem)
            {
                var resultFactory = GameManager.Instance.GetService<IResultFactory>();
                if (!string.IsNullOrWhiteSpace(magicalItem.ShapeTypeName))
                {
                    magicalItem.Shape = resultFactory.GetShape(magicalItem.ShapeTypeName);
                }
                if (!string.IsNullOrWhiteSpace(magicalItem.TargetingTypeName))
                {
                    magicalItem.Targeting = resultFactory.GetTargeting(magicalItem.TargetingTypeName);
                }
            }

            if (item.EffectIds != null && item.EffectIds.Length > 0 && item.Effects == null)
            {
                item.Effects = item.EffectIds.Select(x => _typeRegistry.GetEffect(new Guid(x))).ToList();
            }
        }

        public InventoryData GetSaveData()
        {
            var equippedItems = _equippedItems
                .Where(x => !(x.Value?.Item?.Id.IsNullOrWhiteSpace() ?? false))
                .Select(x => new Api.Utilities.Data.KeyValuePair<string, string>(
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
                Gadgets = groupedItems.FirstOrDefault(x => x.Key == typeof(Gadget))?.Select(x => x as Gadget).ToArray(),
                Spells = groupedItems.FirstOrDefault(x => x.Key == typeof(Spell))?.Select(x => x as Spell).ToArray(),
                Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray(),
                EquippedItems = equippedItems.ToArray()
            };
        }

        public KeyValuePair<SlotGameObjectName, EquippedItem>? GetEquippedWithItemId(string itemId)
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
                return new List<string> { _localizer.Translate("crafting.error.nocomponents") };
            }

            var components = GetComponentsFromIds(componentIds);

            var errors = new List<string>();
            if (itemToCraft is Spell spell)
            {
                if (spell.EffectIds.Length == 0)
                {
                    errors.Add(_localizer.Translate("crafting.error.spellmissingeffect"));
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

        //todo: generalise for use in LivingEntityBase
        public void SpawnEquippedObject(ItemBase item, SlotGameObjectName slotGameObjectName)
        {
            DespawnEquippedObject(slotGameObjectName);

            var isLeftHand = slotGameObjectName == SlotGameObjectName.LeftHand;

            if (item == null)
            {
                return;
            }

            switch (slotGameObjectName)
            {
                case SlotGameObjectName.LeftHand:
                case SlotGameObjectName.RightHand:
                    SpawnItemInHand(slotGameObjectName, item, isLeftHand);
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

                    _typeRegistry.LoadAddessable(
                        weapon.IsTwoHanded ? weaponRegistryType.PrefabAddressTwoHanded : weaponRegistryType.PrefabAddress,
                        prefab =>
                        {
                            InstantiateInPlayerHand(prefab, isLeftHand, new Vector3(0, 90), slotGameObjectName);
                        }
                    );

                    break;

                case Spell:
                case Gadget:
                    var prefab = item is Spell
                        ? GameManager.Instance.Prefabs.Combat.SpellInHand
                        : GameManager.Instance.Prefabs.Combat.GadgetInHand;

                    InstantiateInPlayerHand(prefab, isLeftHand, null, slotGameObjectName);

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

            _typeRegistry.LoadAddessable(
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

            _typeRegistry.LoadAddessable(
                registryType.PrefabAddress,
                prefab =>
                {
                    var newObj = Instantiate(prefab, parentTransform);
                    _equippedItems[slotGameObjectName].GameObject = newObj;
                });
        }

        public void AddItemAsAdmin(ItemBase item)
        {
            GameManager.Instance.CheckIsAdmin();

            FillTypesFromIds(item);
            _items.Add(item.Id, item);
        }
    }
}
