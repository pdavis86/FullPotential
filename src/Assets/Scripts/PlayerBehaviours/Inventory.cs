using Assets.Scripts.Crafting.Results;
using Assets.Scripts.Data;
using Assets.Scripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class Inventory : NetworkBehaviour
{
    public int MaxItems;

    [HideInInspector]
    public List<ItemBase> Items;

    [HideInInspector]
    public string[] EquippedItems;

    private PlayerController _playerController;

    private void Awake()
    {
        Items = new List<ItemBase>();
        EquippedItems = new string[6];
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            var netId = GetComponent<NetworkIdentity>();
            if (netId == null)
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

        if (_playerController.HasMenuOpen && GameManager.Instance.MainCanvasObjects.CraftingUi.activeSelf)
        {
            var uiScript = GameManager.Instance.MainCanvasObjects.CraftingUi.GetComponent<CraftingUi>();
            uiScript.ResetUi();
            uiScript.LoadInventory();
        }
    }

    public void ApplyChanges(Assets.Scripts.Data.Inventory changes)
    {
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

        var addedItemsCount = addedItems.Count();

        if (addedItemsCount == 1)
        {
            var addedItem = addedItems.First();

            //todo: make this a slide-out alert instead
            Debug.LogError($"{addedItem.GetFullName()} was added");
        }
        else
        {
            //todo: make this a slide-out alert instead
            Debug.LogError($"Added {addedItemsCount} items to the inventory after handling message on " + (isServer ? "server" : "client") + " for " + gameObject.name);
        }

        if (changes.EquipSlots.Length == EquippedItems.Length)
        {
            EquippedItems = changes.EquipSlots;
        }
        else
        {
            Debug.LogError("EquipSlots differed in length from EquippedItems");

            if (changes.EquipSlots.Length > EquippedItems.Length)
            {
                EquippedItems = changes.EquipSlots;
            }
            else
            {
                for (var i = 0; i < changes.EquipSlots.Length; i++)
                {
                    EquippedItems[i] = changes.EquipSlots[i];
                }
            }
        }
    }

    private void ApplyChanges(Assets.Scripts.Data.InventoryChange changes)
    {
        ApplyChanges(changes as Assets.Scripts.Data.Inventory);

        if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
        {
            var itemsRemoved = Items.RemoveAll(x => changes.IdsToRemove.Contains(x.Id));

            //todo: make this a slide-out alert instead
            Debug.Log($"Removed {itemsRemoved} items from the inventory after handling message on " + (isServer ? "server" : "client") + " for " + gameObject.name);
        }
    }

    public Assets.Scripts.Data.Inventory GetSaveData()
    {
        var groupedItems = Items.GroupBy(x => x.GetType());

        return new Assets.Scripts.Data.Inventory
        {
            MaxItems = MaxItems,
            Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(ItemBase))?.ToArray(),
            Accessories = groupedItems.FirstOrDefault(x => x.Key == typeof(Accessory))?.Select(x => x as Accessory).ToArray() as Accessory[],
            Armor = groupedItems.FirstOrDefault(x => x.Key == typeof(Armor))?.Select(x => x as Armor).ToArray() as Armor[],
            Spells = groupedItems.FirstOrDefault(x => x.Key == typeof(Spell))?.Select(x => x as Spell).ToArray() as Spell[],
            Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray() as Weapon[],
            EquipSlots = EquippedItems
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
            //todo: send to storage instead
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
        CmdSetItemToSlot(slotName, itemId);
    }

    public void SetItemToSlot(string slotName, string itemId)
    {
        //todo: 
        //var alreadyEquiped = Equipped.FirstOrDefault(x => x.Value == itemId);
        //if (!string.IsNullOrWhiteSpace(alreadyEquiped.Key))
        //{
        //    Equipped.Remove(alreadyEquiped.Key);
        //}


        //todo: do this better
        if (slotName == "Left Hand")
        {
            EquippedItems[0] = itemId;
        }
        else
        {
            EquippedItems[1] = itemId;
        }
    }

    [Command]
    private void CmdSetItemToSlot(string slotName, string itemId)
    {
        SetItemToSlot(slotName, itemId);
    }

}
