using Assets.Core.Crafting;
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

public class Inventory : NetworkBehaviour
{
    public int MaxItems;

    [HideInInspector]
    public List<ItemBase> Items;

    [HideInInspector]
    public string[] EquipSlots;

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
        Gloves,
        Belt,
        Amulet
    }

    private void Awake()
    {
        Items = new List<ItemBase>();

        //Must be the same length as SlotIndexToGameObjectName
        EquipSlots = new string[12];
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
            connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryChange, OnInventoryChange);
        }

        _playerController = GetComponent<PlayerController>();
    }

    private void OnInventoryChange(NetworkMessage netMsg)
    {
        //Debug.LogError("Recieved OnInventoryChange network message");

        var changes = JsonUtility.FromJson<InventoryChange>(netMsg.ReadMessage<StringMessage>().value);
        ApplyChanges(changes);
    }

    public bool ApplyChanges(Assets.Core.Data.Inventory changes, bool firstSetup = false)
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

            var addedItems = changes.Loot
                .UnionIfNotNull(changes.Accessories)
                .UnionIfNotNull(changes.Armor)
                .UnionIfNotNull(changes.Spells)
                .UnionIfNotNull(changes.Weapons);

            Items.AddRange(addedItems);

            if (!firstSetup)
            {
                var addedItemsCount = addedItems.Count();

                if (addedItemsCount == 1)
                {
                    var addedItem = addedItems.First();

                    //todo: make this a slide-out alert instead
                    Debug.LogWarning($"{addedItem.GetFullName()} was added");
                }
                else
                {
                    //todo: make this a slide-out alert instead
                    Debug.LogWarning($"Added {addedItemsCount} items to the inventory after handling message on " + (isServer ? "server" : "client") + " for " + gameObject.name);
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

    private void HandleOldSaveFile(Assets.Core.Data.Inventory changes)
    {
        if (changes.EquipSlots.Length != EquipSlots.Length)
        {
            Debug.LogWarning("Incoming EquipSlots length differed to existing");

            if (changes.EquipSlots.Length > EquipSlots.Length)
            {
                EquipSlots = changes.EquipSlots;
            }
            else
            {
                for (var i = 0; i < changes.EquipSlots.Length; i++)
                {
                    EquipSlots[i] = changes.EquipSlots[i];
                }
            }
        }
    }

    private void ApplyChanges(InventoryChange changes)
    {
        ApplyChanges(changes as Assets.Core.Data.Inventory);

        if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
        {
            var itemsRemoved = Items.RemoveAll(x => changes.IdsToRemove.Contains(x.Id));

            //todo: make this a slide-out alert instead
            Debug.LogWarning($"Removed {itemsRemoved} items from the inventory after handling message on " + (isServer ? "server" : "client") + " for " + gameObject.name);
        }
    }

    public Assets.Core.Data.Inventory GetSaveData()
    {
        var groupedItems = Items.GroupBy(x => x.GetType());

        return new Assets.Core.Data.Inventory
        {
            MaxItems = MaxItems,
            Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(ItemBase))?.ToArray(),
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
            //Debug.Log("Your inventory is at max");
            return;
        }

        Items.Add(item);

        if (!isLocalPlayer)
        {
            var changeJson = JsonUtility.ToJson(new InventoryChange { Loot = new ItemBase[] { item } });
            connectionToClient.Send(Assets.Scripts.Networking.MessageIds.InventoryChange, new StringMessage(changeJson));
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

        if (components.Count != componentIds.Count())
        {
            Debug.LogError("One or more IDs provided are not in the inventory");
            return;
        }

        var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(components, selectedType, selectedSubtype, isTwoHanded);

        var craftedType = craftedItem.GetType();

        var invChange = new InventoryChange
        {
            IdsToRemove = componentIds.ToArray(),
            Accessories = craftedType == typeof(Accessory) ? new Accessory[] { craftedItem as Accessory } : null,
            Armor = craftedType == typeof(Armor) ? new Armor[] { craftedItem as Armor } : null,
            Spells = craftedType == typeof(Spell) ? new Spell[] { craftedItem as Spell } : null,
            Weapons = craftedType == typeof(Weapon) ? new Weapon[] { craftedItem as Weapon } : null
        };

        ApplyChanges(invChange);

        if (!isLocalPlayer)
        {
            var itemJson = JsonUtility.ToJson(invChange);
            connectionToClient.Send(Assets.Scripts.Networking.MessageIds.InventoryChange, new StringMessage(itemJson));
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
        if (!Enum.TryParse<SlotIndexToGameObjectName>(slotName, out var slotReult))
        {
            Debug.LogError($"Failed to find slot for name {slotName}");
            return;
        }

        var equippedIndex = Array.IndexOf(EquipSlots, itemId);
        if (equippedIndex >= 0)
        {
            //Debug.Log($"{itemId} is already assigned to slot {equippedIndex}");
            EquipSlots[equippedIndex] = null;
        }

        EquipSlots[(int)slotReult] = itemId;
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

        return item is Spell ? item as Spell : null;
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
