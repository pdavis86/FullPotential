using FullPotential.Api.Registry;
using FullPotential.Core.Data;
using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using FullPotential.Core.Registry.Base;
using FullPotential.Core.Registry.Types;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using FullPotential.Api.Combat;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.Ui;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ConvertToUsingDeclaration
namespace FullPotential.Core.Behaviours.PlayerBehaviours
{
    public class PlayerInventory : NetworkBehaviour, IDefensible
    {
        public enum SlotGameObjectName
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

        // ReSharper disable InconsistentNaming
        private readonly NetworkVariable<FixedString32Bytes> EquippedHelm = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString32Bytes> EquippedChest = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString32Bytes> EquippedLegs = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString32Bytes> EquippedFeet = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString32Bytes> EquippedBarrier = new NetworkVariable<FixedString32Bytes>();
        public readonly NetworkVariable<FixedString32Bytes> EquippedLeftHand = new NetworkVariable<FixedString32Bytes>();
        public readonly NetworkVariable<FixedString32Bytes> EquippedRightHand = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString32Bytes> EquippedLeftRing = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString32Bytes> EquippedRightRing = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString32Bytes> EquippedBelt = new NetworkVariable<FixedString32Bytes>();
        private readonly NetworkVariable<FixedString32Bytes> EquippedAmulet = new NetworkVariable<FixedString32Bytes>();
        // ReSharper restore InconsistentNaming

        [SerializeField] private float _amuletForwardMultiplier = 0.2f;

        private PlayerState _playerState;
        private Dictionary<string, ItemBase> _items;
        private Dictionary<SlotGameObjectName, GameObject> _equippedObjects;
        private Dictionary<NetworkVariable<FixedString32Bytes>, SlotGameObjectName> _variableToSlotNameMapping;
        private Dictionary<SlotGameObjectName, NetworkVariable<FixedString32Bytes>> _slotNameToVariableMapping;
        private int _slotCount;
        private int _armorSlotCount;
        private int _maxItems;
        private bool _inventoryLoaded;
        private ClientRpcParams _clientRpcParams;

        #region Event Handlers

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerState = GetComponent<PlayerState>();

            _items = new Dictionary<string, ItemBase>();
            _equippedObjects = new Dictionary<SlotGameObjectName, GameObject>();

            SetupMappings();

            _slotCount = Enum.GetNames(typeof(SlotGameObjectName)).Length;
            _armorSlotCount = Enum.GetNames(typeof(IGearArmor.ArmorCategory)).Length;

            EquippedHelm.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedHelm, x, y);
            EquippedChest.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedChest, x, y);
            EquippedLegs.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedLegs, x, y);
            EquippedFeet.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedFeet, x, y);
            EquippedBarrier.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedBarrier, x, y);
            EquippedLeftHand.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedLeftHand, x, y);
            EquippedRightHand.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedRightHand, x, y);
            EquippedLeftRing.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedLeftRing, x, y);
            EquippedRightRing.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedRightRing, x, y);
            EquippedBelt.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedBelt, x, y);
            EquippedAmulet.OnValueChanged += (x, y) => OnEquippedValueChanged(EquippedAmulet, x, y);
        }

        // ReSharper disable once UnusedParameter.Local
        private void OnEquippedValueChanged(
            NetworkVariable<FixedString32Bytes> variable,
            FixedString32Bytes previousValue,
            FixedString32Bytes newValue)
        {
            if (!_inventoryLoaded)
            {
                return;
            }

            var slot = GetSlotNameFromVariable(variable);
            SpawnEquippedObject(newValue.Value, slot);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _clientRpcParams.Send.TargetClientIds = new[] { OwnerClientId };
        }

        #endregion

        #region ServerRpc calls

        [ServerRpc]
        public void EquipItemServerRpc(string itemId, SlotGameObjectName slot, bool allowUnEquip)
        {
            EquipItem(itemId, slot, allowUnEquip);

            var saveData = GameManager.Instance.UserRegistry.PlayerData[_playerState.Username];
            saveData.Inventory = GetSaveData();
            saveData.IsDirty = true;

            ResetEquipmentUiClientRpc(_clientRpcParams);
        }

        #endregion

        #region ClientRpc calls

        [ClientRpc]
        // ReSharper disable once UnusedParameter.Local
        private void ResetEquipmentUiClientRpc(ClientRpcParams clientRpcParams)
        {
            StartCoroutine(ResetEquipmentUi());
        }

        #endregion

        private IEnumerator ResetEquipmentUi()
        {
            yield return new WaitForSeconds(0.1f);

            var characterMenuUi = GameManager.Instance.MainCanvasObjects.CharacterMenu.GetComponent<CharacterMenuUi>();
            var equipmentUi = characterMenuUi.Equipment.GetComponent<CharacterMenuUiEquipmentTab>();

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
                var equippedItemId = GetVariableFromSlotName(slotGameObjectName).Value.ToString();

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

        public int GetSlotCount()
        {
            return _slotCount;
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
                    {
                        var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemadded");
                        _playerState.ShowAlertForItemsAddedToInventory(string.Format(alertText, itemsToAdd.First().Name));
                        break;
                    }
                case > 1:
                    {
                        var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemsadded");
                        _playerState.ShowAlertForItemsAddedToInventory(string.Format(alertText, itemToAddCount));
                        break;
                    }
            }

            var saveData = GameManager.Instance.UserRegistry.PlayerData[_playerState.Username];
            saveData.Inventory = GetSaveData();
            saveData.IsDirty = true;
        }

        public void LoadInventory(Inventory inventoryData)
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

            if (IsServer)
            {
                SetEquippedVariables(inventoryData.EquippedItems);
            }

            foreach (SlotGameObjectName slotGameObjectName in Enum.GetValues(typeof(SlotGameObjectName)))
            {
                var variable = GetVariableFromSlotName(slotGameObjectName);
                SpawnEquippedObject(variable.Value.Value, slotGameObjectName);
            }

            _inventoryLoaded = true;
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

        public NetworkVariable<FixedString32Bytes> GetVariableSetToItemId(string itemId)
        {
            return Enum.GetValues(typeof(SlotGameObjectName))
                .Cast<SlotGameObjectName>()
                .Select(GetVariableFromSlotName)
                .FirstOrDefault(variable => variable.Value.ToString() == itemId);
        }

        //private IEnumerable<NetworkVariable<FixedString32Bytes>> GetVariablesSetToItemId(string itemId)
        //{
        //    return Enum.GetValues(typeof(SlotGameObjectName))
        //        .Cast<SlotGameObjectName>()
        //        .Select(GetVariableFromSlotName)
        //        .Where(variable => variable.Value.ToString() == itemId);
        //}

        private Inventory GetSaveData()
        {
            var equippedItems = new List<Data.KeyValuePair<string, string>>();
            for (var i = 0; i < _slotCount; i++)
            {
                var equippedItemId = GetVariableFromSlotName((SlotGameObjectName)i).Value.ToString();
                if (!equippedItemId.IsNullOrWhiteSpace())
                {
                    var slotName = Enum.GetName(typeof(SlotGameObjectName), i);
                    equippedItems.Add(new Data.KeyValuePair<string, string>(slotName, equippedItemId));
                }
            }

            var groupedItems = _items
                .Select(x => x.Value)
                .GroupBy(x => x.GetType());

            return new Inventory
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

        private void SetupMappings()
        {
            _slotNameToVariableMapping = new Dictionary<SlotGameObjectName, NetworkVariable<FixedString32Bytes>>
            {
                { SlotGameObjectName.Helm, EquippedHelm },
                { SlotGameObjectName.Chest, EquippedChest },
                { SlotGameObjectName.Legs, EquippedLegs },
                { SlotGameObjectName.Feet, EquippedFeet },
                { SlotGameObjectName.Barrier, EquippedBarrier },
                { SlotGameObjectName.LeftHand, EquippedLeftHand },
                { SlotGameObjectName.RightHand, EquippedRightHand },
                { SlotGameObjectName.LeftRing, EquippedLeftRing },
                { SlotGameObjectName.RightRing, EquippedRightRing },
                { SlotGameObjectName.Belt, EquippedBelt },
                { SlotGameObjectName.Amulet, EquippedAmulet }
            };

            _variableToSlotNameMapping = _slotNameToVariableMapping.ToDictionary(
                kvp => kvp.Value,
                kvp => kvp.Key
                );
        }

        private NetworkVariable<FixedString32Bytes> GetVariableFromSlotName(SlotGameObjectName slotName)
        {
            if (!_slotNameToVariableMapping.ContainsKey(slotName))
            {
                throw new ArgumentException($"Unexpected slot name {slotName}");
            }

            return _slotNameToVariableMapping[slotName];
        }

        private SlotGameObjectName GetSlotNameFromVariable(NetworkVariable<FixedString32Bytes> variable)
        {
            if (!_variableToSlotNameMapping.ContainsKey(variable))
            {
                throw new ArgumentException($"Unexpected variable {variable}");
            }

            return _variableToSlotNameMapping[variable];
        }

        public ItemBase GetItemInSlot(SlotGameObjectName slotName)
        {
            return GetItemWithId<ItemBase>(
                GetVariableFromSlotName(slotName).Value.ToString(),
                false);
        }

        public ItemBase GetItemInHand(bool isLeftHand)
        {
            var idInHand = isLeftHand
                ? EquippedLeftHand.Value.ToString()
                : EquippedRightHand.Value.ToString();

            if (idInHand.IsNullOrWhiteSpace())
            {
                return null;
            }

            return GetItemWithId<ItemBase>(idInHand, false);
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

        private void SetEquippedVariables(Data.KeyValuePair<string, string>[] equippedItems)
        {
            if (!IsServer)
            {
                Debug.LogError("Cannot set equipped items on the client");
                return;
            }

            if (equippedItems == null)
            {
                //Debug.LogWarning("No equipped items provided");
                return;
            }

            foreach (var equippedItem in equippedItems)
            {
                if (equippedItem.Key.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if (!Enum.TryParse<SlotGameObjectName>(equippedItem.Key, out var slotResult))
                {
                    Debug.LogError($"Failed to load slot data for {equippedItem.Key}");
                }

                GetVariableFromSlotName(slotResult).Value = equippedItem.Value;
            }
        }

        private void EquipItem(string itemId, SlotGameObjectName slot, bool allowUnEquip)
        {
            if (!IsServer)
            {
                Debug.LogError("Cannot equip items on the client");
                return;
            }

            var oldVariable = GetVariableSetToItemId(itemId);

            if (!string.IsNullOrWhiteSpace(itemId) && allowUnEquip)
            {

                if (oldVariable != null)
                {
                    oldVariable.Value = string.Empty;
                }
            }

            var newVariable = GetVariableFromSlotName(slot);

            if (oldVariable != newVariable)
            {
                newVariable.Value = itemId.IsNullOrWhiteSpace()
                    ? string.Empty
                    : itemId;
            }
        }

        private void DespawnEquippedObject(SlotGameObjectName slotGameObjectName)
        {
            if (!_equippedObjects.ContainsKey(slotGameObjectName))
            {
                return;
            }

            var currentlyInGame = _equippedObjects[slotGameObjectName];

            if (currentlyInGame == null)
            {
                return;
            }

            currentlyInGame.name = "DESTROY" + currentlyInGame.name;
            Destroy(currentlyInGame);
            _equippedObjects[slotGameObjectName] = null;
        }

        private void SpawnEquippedObject(string itemId, SlotGameObjectName slotGameObjectName)
        {
            DespawnEquippedObject(slotGameObjectName);

            if (itemId.IsNullOrWhiteSpace())
            {
                if (IsOwner && IsClient)
                {
                    switch (slotGameObjectName)
                    {
                        case SlotGameObjectName.LeftHand:
                        case SlotGameObjectName.RightHand:
                            GameManager.Instance.MainCanvasObjects.Hud.GetComponent<Hud>().UpdateHand(null, slotGameObjectName == SlotGameObjectName.LeftHand);
                            break;
                    }
                }

                return;
            }

            var item = GetItemWithId<ItemBase>(itemId);

            switch (slotGameObjectName)
            {
                case SlotGameObjectName.LeftHand:
                case SlotGameObjectName.RightHand:
                    SpawnItemInHand(slotGameObjectName, item, slotGameObjectName == SlotGameObjectName.LeftHand);
                    break;

                case SlotGameObjectName.Amulet:
                    InstantiateAccessory(item, _amuletForwardMultiplier);
                    break;

                case SlotGameObjectName.Belt:
                    InstantiateAccessory(item);
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
                GameManager.Instance.MainCanvasObjects.Hud.GetComponent<Hud>().UpdateHand(contents, isLeftHand);
            }

            if (!NetworkManager.Singleton.IsClient)
            {
                Debug.LogError("Tried to spawn a GameObject on a server");
                return;
            }

            switch (item)
            {
                case Weapon weapon:
                    if (item.RegistryType is not IGearWeapon registryType)
                    {
                        Debug.LogError("Item did not have a RegistryType");
                        return;
                    }

                    GameManager.Instance.TypeRegistry.LoadAddessable(
                        weapon.IsTwoHanded ? registryType.PrefabAddressTwoHanded : registryType.PrefabAddress,
                        prefab =>
                        {
                            InstantiateInPlayerHand(prefab, isLeftHand, new Vector3(0, 90), slotGameObjectName);
                        }
                    );
                    break;

                case Spell:
                    InstantiateInPlayerHand(GameManager.Instance.Prefabs.Combat.SpellInHand, isLeftHand, null, slotGameObjectName);
                    break;

                default:
                    Debug.LogWarning($"Not implemented SpawnItemInHand handling for item type {item.GetType().Name}");
                    _equippedObjects[slotGameObjectName] = null;
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

            _equippedObjects[slotGameObjectName] = newObj;
        }

        private void InstantiateAccessory(ItemBase item, float forwardMultiplier = 0)
        {
            if (NetworkManager.LocalClientId == OwnerClientId)
            {
                return;
            }

            if (item.RegistryType is not IGearAccessory registryType)
            {
                Debug.LogError("Item did not have a RegistryType");
                return;
            }

            GameManager.Instance.TypeRegistry.LoadAddessable(
                registryType.PrefabAddress,
                prefab =>
                {
                    var newObj = Instantiate(prefab, _playerState.GraphicsTransform);
                    newObj.transform.position += newObj.transform.forward * forwardMultiplier;
                    _equippedObjects[SlotGameObjectName.Amulet] = newObj;
                });
        }

    }
}
