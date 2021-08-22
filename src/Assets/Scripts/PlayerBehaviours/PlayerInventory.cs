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

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ConvertToUsingDeclaration

public class PlayerInventory : NetworkBehaviour
{
    public int MaxItems = 30;

    [HideInInspector]
    public List<ItemBase> Items;

    [HideInInspector]
    public string[] EquipSlots;

    [HideInInspector]
    public GameObject[] EquippedObjects;

    private PlayerSetup _playerSettings;
    private PlayerController _playerController;
    private CraftingUi _craftingUi;

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

    private void Awake()
    {
        Items = new List<ItemBase>();

        var slotCount = Enum.GetNames(typeof(SlotIndexToGameObjectName)).Length;
        EquipSlots = new string[slotCount];
        EquippedObjects = new GameObject[slotCount];
    }

    private void Start()
    {
        _playerSettings = GetComponent<PlayerSetup>();
        _playerController = GetComponent<PlayerController>();

        _craftingUi = GameManager.Instance.MainCanvasObjects.CraftingUi.GetComponent<CraftingUi>();

        if (IsLocalPlayer)
        {
            CustomMessagingManager.RegisterNamedMessageHandler(nameof(Assets.Core.Networking.MessageType.InventoryChange), OnInventoryChange);
        }
    }

    private void OnInventoryChange(ulong clientId, System.IO.Stream stream)
    {
        //Debug.LogError("Recieved OnInventoryChange network message");

        string message;
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            message = reader.ReadString().ToString();
        }

        var changes = JsonUtility.FromJson<InventoryAndRemovals>(message);
        ApplyInventoryAndRemovals(changes);
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
                MaxItems = changes.MaxItems;
            }

            var addedItems = Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(changes.Loot)
                .UnionIfNotNull(changes.Accessories)
                .UnionIfNotNull(changes.Armor)
                .UnionIfNotNull(changes.Spells)
                .UnionIfNotNull(changes.Weapons);

            Items.AddRange(addedItems);

            foreach (var item in Items)
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

            //todo: this method is a mix of server-side and client-side logic!

            //todo: 
            //if (!firstSetup)
            //{
            //    var addedItemsCount = addedItems.Count();

            //    if (addedItemsCount == 1)
            //    {
            //        var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemadded");
            //        _playerController.ShowAlert(string.Format(alertText, addedItems.First().Name));
            //    }
            //    else
            //    {
            //        var alertText = GameManager.Instance.Localizer.Translate("ui.alert.itemsadded");
            //        _playerController.ShowAlert(string.Format(alertText, addedItemsCount));
            //    }
            //}

            if (changes.EquipSlots != null && changes.EquipSlots.Any())
            {
                if (changes.EquipSlots.Length == EquipSlots.Length)
                {
                    EquipSlots = changes.EquipSlots;
                }
                else
                {
                    HandleOldSaveFile(changes);
                }

                //DebugLogEquippedItems();
            }

            //todo:
            //EquipItems();

            //if (_playerController != null && _playerController.HasMenuOpen && GameManager.Instance.MainCanvasObjects.CraftingUi.activeSelf)
            //{
            //    _craftingUi.ResetUi();
            //    _craftingUi.LoadInventory();
            //}

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            return false;
        }
    }

    private void HandleOldSaveFile(Inventory changes)
    {
        if (changes.EquipSlots.Length != EquipSlots.Length)
        {
            Debug.LogWarning("Incoming EquipSlots length differed to existing");

            for (var i = 0; i < Math.Min(EquipSlots.Length, changes.EquipSlots.Length); i++)
            {
                EquipSlots[i] = changes.EquipSlots[i];
            }
        }
    }

    private void ApplyInventoryAndRemovals(InventoryAndRemovals changes)
    {
        if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
        {
            var itemsRemoved = Items.RemoveAll(x => changes.IdsToRemove.Contains(x.Id));

            //todo: make this a slide-out alert instead
            Debug.LogWarning($"Removed {itemsRemoved} items from the inventory after handling message on " + (IsServer ? "server" : "client") + " for " + gameObject.name);
        }

        ApplyInventory(changes);
    }

    public Inventory GetSaveData()
    {
        //todo: feasible to use separate lists for different types of item?
        var groupedItems = Items.GroupBy(x => x.GetType());

        return new Inventory
        {
            MaxItems = MaxItems,
            Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(Loot))?.Select(x => x as Loot).ToArray(),
            Accessories = groupedItems.FirstOrDefault(x => x.Key == typeof(Accessory))?.Select(x => x as Accessory).ToArray(),
            Armor = groupedItems.FirstOrDefault(x => x.Key == typeof(Armor))?.Select(x => x as Armor).ToArray(),
            Spells = groupedItems.FirstOrDefault(x => x.Key == typeof(Spell))?.Select(x => x as Spell).ToArray(),
            Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray(),
            EquipSlots = EquipSlots
        };
    }

    //private void AddOfType<T>(string stringValue) where T : ItemBase
    //{
    //    Add(JsonUtility.FromJson<T>(stringValue));
    //    //Debug.Log($"Inventory now has {Items.Count} items in it");
    //}

    public void Add(ItemBase item)
    {
        //Debug.LogError("Inventory add called on the " + (IsServer ? "server" : "client"));

        if (Items.Count == MaxItems)
        {
            //todo: replace with client-side message
            Debug.Log("Your inventory is at max");
            return;
        }

        //todo: what type is the item being added?
        var change = new InventoryAndRemovals { Loot = new[] { item as Loot } };

        ApplyInventoryAndRemovals(change);

        var json = JsonUtility.ToJson(change);

        //todo: are these both the same?
        var clientId = OwnerClientId;
        clientId = _playerSettings.ClientId.Value;

        var stream = PooledNetworkBuffer.Get();
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteString(json);
            CustomMessagingManager.SendNamedMessage(nameof(Assets.Core.Networking.MessageType.InventoryChange), clientId, stream);
        }
    }

    public void RemoveIds(IEnumerable<string> idEnumerable)
    {
        foreach (var id in idEnumerable)
        {
            var matchingItem = Items.FirstOrDefault(x => x.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase));
            if (matchingItem == null)
            {
                Debug.LogError("No item found with ID: " + id);
                continue;
            }
            Items.Remove(matchingItem);
        }
    }

    private List<ItemBase> GetComponentsFromIds(string[] componentIds)
    {
        //Check that the components are actually in the player's inventory and load them in the order they are given
        var components = new List<ItemBase>();
        foreach (var id in componentIds)
        {
            components.Add(Items.FirstOrDefault(x => x.Id == id));
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

    [ServerRpc]
    public void CraftItemServerRpc(string[] componentIds, string categoryName, string craftableTypeName, bool isTwoHanded, string itemName)
    {
        var components = GetComponentsFromIds(componentIds);

        if (components.Count != componentIds.Length)
        {
            Debug.LogError("Someone tried cheating: One or more IDs provided are not in the inventory");
            return;
        }

        var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(
            categoryName,
            craftableTypeName,
            isTwoHanded,
            components
        );

        if (ValidateIsCraftable(componentIds, craftedItem).Any())
        {
            Debug.LogError("Someone tried cheating: validation was skipped");
            return;
        }

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            craftedItem.Name = itemName;
        }

        var craftedType = craftedItem.GetType();

        var invChange = new InventoryAndRemovals
        {
            IdsToRemove = componentIds.ToArray(),
            Accessories = craftedType == typeof(Accessory) ? new[] { craftedItem as Accessory } : null,
            Armor = craftedType == typeof(Armor) ? new[] { craftedItem as Armor } : null,
            Spells = craftedType == typeof(Spell) ? new[] { craftedItem as Spell } : null,
            Weapons = craftedType == typeof(Weapon) ? new[] { craftedItem as Weapon } : null
        };

        ApplyInventoryAndRemovals(invChange);

        var json = JsonUtility.ToJson(invChange);

        var clientId = OwnerClientId;
        clientId = _playerSettings.ClientId.Value;

        var stream = PooledNetworkBuffer.Get();
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteString(json);
            CustomMessagingManager.SendNamedMessage(nameof(Assets.Core.Networking.MessageType.InventoryChange), clientId, stream);
        }
    }

    public void SetItemToSlotOnBoth(string slotName, string itemId)
    {
        SetItemToSlot(slotName, itemId);
        if (!IsServer)
        {
            SetItemToSlotServerRpc(slotName, itemId);
        }
    }

    public void SetItemToSlot(string slotName, string itemId)
    {
        if (!Enum.TryParse<SlotIndexToGameObjectName>(slotName, out var slotResult))
        {
            Debug.LogError($"Failed to find slot for name {slotName}");
            return;
        }

        var equippedIndex = Array.IndexOf(EquipSlots, itemId);
        if (equippedIndex >= 0)
        {
            //Debug.Log($"{itemId} is already assigned to slot {equippedIndex}");
            //EquipSlots[equippedIndex] = null;
            EquipItem(equippedIndex, null);
        }

        //EquipSlots[(int)slotResult] = itemId;
        EquipItem((int)slotResult, itemId);
    }

    [ServerRpc]
    private void SetItemToSlotServerRpc(string slotName, string itemId)
    {
        SetItemToSlot(slotName, itemId);
    }

    public Spell GetSpellInHand(bool leftHand)
    {
        var itemId = leftHand
            ? EquipSlots[(int)SlotIndexToGameObjectName.LeftHand]
            : EquipSlots[(int)SlotIndexToGameObjectName.RightHand];

        var item = Items.FirstOrDefault(x => x.Id == itemId);

        return item as Spell;
    }

    public void SpawnItemInHand(int index, ItemBase item, bool leftHand = true)
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
                    var weaponGo = Instantiate(prefab, gameObject.transform);
                    weaponGo.transform.localEulerAngles = new Vector3(0, 90);
                    weaponGo.transform.localPosition = new Vector3(leftHand ? -0.38f : 0.38f, 0.3f, 0.75f);
                    EquippedObjects[index] = weaponGo;
                }
                );
        }
        else
        {
            //todo: implement other items
            Debug.LogWarning($"Not implemented handling for item {item.Name} yet");
        }
    }

    public void EquipItem(int slotIndex, string itemId)
    {
        var currentlyInGame = EquippedObjects[slotIndex];
        if (currentlyInGame != null)
        {
            Destroy(currentlyInGame);
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            EquipSlots[slotIndex] = null;
            return;
        }

        EquipSlots[slotIndex] = itemId;

        if (slotIndex == (int)SlotIndexToGameObjectName.LeftHand)
        {
            SpawnItemInHand(slotIndex, Items.First(x => x.Id == itemId));
        }
        else if (slotIndex == (int)SlotIndexToGameObjectName.RightHand)
        {
            SpawnItemInHand(slotIndex, Items.First(x => x.Id == itemId), false);
        }
    }

    private void EquipItems()
    {
        for (var i = 0; i < EquipSlots.Length; i++)
        {
            EquipItem(i, EquipSlots[i]);
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
