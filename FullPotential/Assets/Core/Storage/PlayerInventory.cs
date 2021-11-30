using FullPotential.Assets.Api.Behaviours;
using FullPotential.Assets.Api.Registry;
using FullPotential.Assets.Core.Data;
using FullPotential.Assets.Core.Extensions;
using FullPotential.Assets.Core.Helpers;
using FullPotential.Assets.Core.Registry.Base;
using FullPotential.Assets.Core.Registry.Types;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable UnusedMember.Local

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

    public readonly NetworkVariable<FixedString32Bytes> EquippedHelm = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedChest = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedLegs = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedFeet = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedBarrier = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedLeftHand = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedRightHand = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedLeftRing = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedRightRing = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedBelt = new NetworkVariable<FixedString32Bytes>();
    public readonly NetworkVariable<FixedString32Bytes> EquippedAmulet = new NetworkVariable<FixedString32Bytes>();

    private PlayerState _playerState;
    private Dictionary<string, ItemBase> _items;
    private Dictionary<SlotGameObjectName, GameObject> _equippedObjects;
    private int _slotCount;
    private int _armorSlotCount;

    private int _maxItems;

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();

        _items = new Dictionary<string, ItemBase>();
        _equippedObjects = new Dictionary<SlotGameObjectName, GameObject>();

        _slotCount = Enum.GetNames(typeof(SlotGameObjectName)).Length;
        _armorSlotCount = Enum.GetNames(typeof(IGearArmor.ArmorCategory)).Length;
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
                .Where(x => x.Value is Weapon || x.Value is Spell)
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
        //todo: all equipped items that implement IDefensible
        //foreach (var i in Enum.GetValues(typeof(SlotGameObjectName)))
        //{
        //    var equippedItemId = GetVariableFromSlotName((SlotGameObjectName)i).Value.ToString();
        //    //todo: finish
        //}

        var equippedArmor = new[] {
                GetItemInSlot(SlotGameObjectName.Helm),
                GetItemInSlot(SlotGameObjectName.Chest),
                GetItemInSlot(SlotGameObjectName.Legs),
                GetItemInSlot(SlotGameObjectName.Feet),
                GetItemInSlot(SlotGameObjectName.Barrier)
            };
        var strengthSum = (float)equippedArmor.Sum(x => x.Attributes.Strength);
        return (int)Math.Floor(strengthSum / _armorSlotCount);
    }

    public int GetSlotCount()
    {
        return _slotCount;
    }

    public void ApplyInventoryChanges(InventoryChanges changes)
    {
        if (!IsServer)
        {
            Debug.LogError($"{nameof(ApplyInventoryChanges)} called on the client!");
            return;
        }

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

        if (itemToAddCount == 1)
        {
            var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemadded");
            _playerState.ShowAlertForItemsAddedToInventory(string.Format(alertText, itemsToAdd.First().Name));
        }
        else if (itemToAddCount > 1)
        {
            var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemsadded");
            _playerState.ShowAlertForItemsAddedToInventory(string.Format(alertText, itemToAddCount));
        }

        SpawnEquippedObjects();
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

        if (inventoryData.EquippedItems != null)
        {
            foreach (var equippedItem in inventoryData.EquippedItems)
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

        SpawnEquippedObjects();
    }

    private void FillTypesFromIds(ItemBase item)
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
        foreach (SlotGameObjectName slotGameObjectName in Enum.GetValues(typeof(SlotGameObjectName)))
        {
            var variable = GetVariableFromSlotName(slotGameObjectName);
            if (variable.Value.ToString() == itemId)
            {
                return variable;
            }
        }

        return null;
    }

    public Inventory GetSaveData()
    {
        var equippedItems = new List<FullPotential.Assets.Core.Data.KeyValuePair<string, string>>();
        for (var i = 0; i < _slotCount; i++)
        {
            var equippedItemId = GetVariableFromSlotName((SlotGameObjectName)i).Value.ToString();
            if (!equippedItemId.IsNullOrWhiteSpace())
            {
                var slotName = Enum.GetName(typeof(SlotGameObjectName), i);
                equippedItems.Add(new FullPotential.Assets.Core.Data.KeyValuePair<string, string>(slotName, equippedItemId));
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

        if (!(item is T))
        {
            throw new Exception($"Item '{id}' was not of the correct type: {typeof(T).Name}");
        }

        return item as T;
    }

    private NetworkVariable<FixedString32Bytes> GetVariableFromSlotName(SlotGameObjectName slotName)
    {
        switch (slotName)
        {
            case SlotGameObjectName.Helm: return EquippedHelm;
            case SlotGameObjectName.Chest: return EquippedChest;
            case SlotGameObjectName.Legs: return EquippedLegs;
            case SlotGameObjectName.Feet: return EquippedFeet;
            case SlotGameObjectName.Barrier: return EquippedBarrier;
            case SlotGameObjectName.LeftHand: return EquippedLeftHand;
            case SlotGameObjectName.RightHand: return EquippedRightHand;
            case SlotGameObjectName.LeftRing: return EquippedLeftRing;
            case SlotGameObjectName.RightRing: return EquippedRightRing;
            case SlotGameObjectName.Belt: return EquippedBelt;
            case SlotGameObjectName.Amulet: return EquippedAmulet;
            default: throw new ArgumentException($"Unexpected slot name {slotName}");
        }
    }

    public ItemBase GetItemInSlot(SlotGameObjectName slotName)
    {
        return GetItemWithId<ItemBase>(
            GetVariableFromSlotName(slotName).Value.ToString(),
            false);
    }

    public ItemBase GetItemInHand(bool isLeftHand)
    {
        return isLeftHand
            ? GetItemWithId<ItemBase>(EquippedLeftHand.Value.ToString())
            : GetItemWithId<ItemBase>(EquippedRightHand.Value.ToString());
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

    public void EquipItem(string itemId, SlotGameObjectName slot, bool allowUnEquip)
    {
        if (!string.IsNullOrWhiteSpace(itemId) && allowUnEquip)
        {
            var oldVariable = GetVariableSetToItemId(itemId);
            if (oldVariable != null)
            {
                oldVariable.Value = string.Empty;
            }
        }

        GetVariableFromSlotName(slot).Value =
            itemId.IsNullOrWhiteSpace()
            ? string.Empty
            : itemId;
    }

    private void SpawnEquippedObjects()
    {
        foreach (SlotGameObjectName slotGameObjectName in Enum.GetValues(typeof(SlotGameObjectName)))
        {
            SpawnEquippedObject(slotGameObjectName);
        }
    }

    public void SpawnEquippedObject(SlotGameObjectName slotGameObjectName)
    {
        var currentlyInGame = _equippedObjects.ContainsKey(slotGameObjectName)
                ? _equippedObjects[slotGameObjectName]
                : null;

        if (currentlyInGame != null)
        {
            currentlyInGame.name = "DESTROY" + currentlyInGame.name;
            Destroy(currentlyInGame);
        }

        var variable = GetVariableFromSlotName(slotGameObjectName);

        if (variable.Value.ToString().IsNullOrWhiteSpace())
        {
            return;
        }

        var item = GetItemWithId<ItemBase>(variable.Value.ToString());

        if (variable == EquippedLeftHand)
        {
            SpawnItemInHand(slotGameObjectName, item);
        }
        else if (variable == EquippedRightHand)
        {
            SpawnItemInHand(slotGameObjectName, item, false);
        }
    }

    private void SpawnItemInHand(SlotGameObjectName slotGameObjectName, ItemBase item, bool isLeftHand = true)
    {
        if (!NetworkManager.Singleton.IsClient)
        {
            Debug.LogError("Tried to spawn a gameobject on a server");
            return;
        }

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
                    InstantiateInPlayerHand(prefab, isLeftHand, false, new Vector3(0, 90), slotGameObjectName);
                }
            );
        }
        else if (item is Spell)
        {
            InstantiateInPlayerHand(GameManager.Instance.Prefabs.Combat.SpellInHand, isLeftHand, true, null, slotGameObjectName);
        }
        else
        {
            Debug.LogError($"Not implemented SpawnItemInHand handling for item type {item.GetType().Name}");
            _equippedObjects[slotGameObjectName] = null;
        }
    }

    private void InstantiateInPlayerHand(GameObject prefab, bool isLeftHand, bool isSpell, Vector3? rotation, SlotGameObjectName slotGameObjectName)
    {
        var newObj = UnityEngine.Object.Instantiate(prefab, _playerState.InFrontOfPlayer.transform);

        newObj.transform.localPosition = isSpell
            ? new Vector3(isLeftHand ? -0.32f : 0.32f, -0.21f, 1.9f)
            : new Vector3(isLeftHand ? -0.38f : 0.38f, -0.25f, 1.9f);

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

}
