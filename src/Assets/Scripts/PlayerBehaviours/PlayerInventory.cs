using Assets.Core;
using Assets.Core.Crafting.Base;
using Assets.Core.Crafting.Types;
using Assets.Core.Data;
using Assets.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable PossibleMultipleEnumeration

public class PlayerInventory : NetworkBehaviour
{
    public int MaxItems = 30;

    [HideInInspector]
    public List<ItemBase> Items;

    [HideInInspector]
    public string[] EquipSlots;

    [HideInInspector]
    public GameObject[] EquippedObjects;

    private PlayerController _playerController;

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
        if (isLocalPlayer)
        {
            var netIdent = GetComponent<NetworkIdentity>();
            if (netIdent == null)
            {
                return;
            }
            connectionToServer.RegisterHandler(Assets.Core.Networking.MessageIds.InventoryChange, OnInventoryChange);
        }

        _playerController = GetComponent<PlayerController>();
    }

    private void OnInventoryChange(NetworkMessage netMsg)
    {
        //Debug.LogError("Recieved OnInventoryChange network message");

        var changes = JsonUtility.FromJson<InventoryChange>(netMsg.ReadMessage<StringMessage>().value);
        ApplyChanges(changes);
    }

    public bool ApplyChanges(Inventory changes, bool firstSetup = false)
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

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            // ReSharper disable once MergeSequentialPatterns
            foreach (CraftableBase craftable in Items.Where(x => x is CraftableBase y && y.CraftableType == null))
            {
                craftable.CraftableType = ApiRegister.Instance.GetCraftableType(craftable);
            }

            foreach (var item in Items.Where(x => x.Effects == null && x.EffectIds != null && x.EffectIds.Length > 0))
            {
                item.Effects = item.EffectIds.Select(x => ApiRegister.Instance.GetEffect(new Guid(x))).ToList();
            }

            if (!firstSetup)
            {
                var addedItemsCount = addedItems.Count();

                if (addedItemsCount == 1)
                {
                    //todo: make this a slide-out alert instead
                    Debug.Log($"{addedItems.First().GetFullName()} was added");
                }
                else
                {
                    //todo: make this a slide-out alert instead
                    Debug.Log($"Added {addedItemsCount} items to the inventory after handling message on " + (isServer ? "server" : "client") + " for " + gameObject.name);
                }
            }

            if (changes.EquipSlots != null)
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

            EquipItems();

            if (_playerController != null && _playerController.HasMenuOpen && GameManager.Instance.MainCanvasObjects.CraftingUi.activeSelf)
            {
                var uiScript = GameManager.Instance.MainCanvasObjects.CraftingUi.GetComponent<CraftingUi>();
                uiScript.ResetUi();
                uiScript.LoadInventory();
            }

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

            for (var i = 0; i < EquipSlots.Length; i++)
            {
                EquipSlots[i] = changes.EquipSlots[i];
            }
        }
    }

    private void ApplyChanges(InventoryChange changes)
    {
        ApplyChanges(changes as Inventory);

        if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
        {
            var itemsRemoved = Items.RemoveAll(x => changes.IdsToRemove.Contains(x.Id));

            //todo: make this a slide-out alert instead
            Debug.LogWarning($"Removed {itemsRemoved} items from the inventory after handling message on " + (isServer ? "server" : "client") + " for " + gameObject.name);
        }
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
        if (Items.Count == MaxItems)
        {
            Debug.Log("Your inventory is at max");
            return;
        }

        //todo: what type is the item being added?
        var change = new InventoryChange { Loot = new [] { item as Loot } };

        ApplyChanges(change);

        if (!isLocalPlayer)
        {
            var changeJson = JsonUtility.ToJson(change);
            connectionToClient.Send(Assets.Core.Networking.MessageIds.InventoryChange, new StringMessage(changeJson));
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

    [Command]
    public void CmdCraftItem(string[] componentIds, string selectedType, string selectedSubtype, bool isTwoHanded)
    {
        //Check that the components are actually in the player's inventory and load them in the order they are given
        var components = new List<ItemBase>();
        foreach (var id in componentIds)
        {
            components.Add(Items.FirstOrDefault(x => x.Id == id));
        }

        if (components.Count != componentIds.Length)
        {
            Debug.LogError("One or more IDs provided are not in the inventory");
            return;
        }

        var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(
            selectedType, 
            selectedSubtype, 
            isTwoHanded, 
            components
        );

        var craftedType = craftedItem.GetType();

        var invChange = new InventoryChange
        {
            IdsToRemove = componentIds.ToArray(),
            Accessories = craftedType == typeof(Accessory) ? new [] { craftedItem as Accessory } : null,
            Armor = craftedType == typeof(Armor) ? new [] { craftedItem as Armor } : null,
            Spells = craftedType == typeof(Spell) ? new [] { craftedItem as Spell } : null,
            Weapons = craftedType == typeof(Weapon) ? new [] { craftedItem as Weapon } : null
        };

        ApplyChanges(invChange);

        if (!isLocalPlayer)
        {
            var itemJson = JsonUtility.ToJson(invChange);
            connectionToClient.Send(Assets.Core.Networking.MessageIds.InventoryChange, new StringMessage(itemJson));
        }
    }

    public void SetItemToSlotOnBoth(string slotName, string itemId)
    {
        SetItemToSlot(slotName, itemId);
        if (!isServer)
        {
            CmdSetItemToSlot(slotName, itemId);
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

    [Command]
    private void CmdSetItemToSlot(string slotName, string itemId)
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
        var currentlyInGame = EquippedObjects[index];
        if (currentlyInGame != null)
        {
            Destroy(currentlyInGame);
        }

        if (item is Weapon weapon)
        {
            var prefab = ApiRegister.GetPrefabForWeaponType(weapon.CraftableType.TypeName, weapon.IsTwoHanded);
            var weaponGo = Instantiate(prefab, gameObject.transform);
            weaponGo.transform.localEulerAngles = new Vector3(0, 90);
            weaponGo.transform.localPosition = new Vector3(leftHand ? -0.38f : 0.38f, 0.3f, 0.75f);
            EquippedObjects[index] = weaponGo;
        }
        else
        {
            //todo: implement other items
            Debug.Log($"Not implemented handling for item {item.Name} yet");
        }
    }

    public void EquipItem(int slotIndex, string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            EquipSlots[slotIndex] = null;
            //todo: destroy game object
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
