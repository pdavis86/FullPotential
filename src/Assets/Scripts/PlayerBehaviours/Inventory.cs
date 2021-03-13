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
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.LoadPlayerData, OnLoadPlayerData);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryChange, OnInventoryChange);
        }

        _playerController = GetComponent<PlayerController>();
    }

    //todo: make a generic load method instead of here
    private void OnLoadPlayerData(NetworkMessage netMsg)
    {
        var loadData = JsonUtility.FromJson<PlayerData>(netMsg.ReadMessage<StringMessage>().value);
        ApplyChanges(loadData.Inventory);
    }

    private void OnInventoryChange(NetworkMessage netMsg)
    {
        var changes = JsonUtility.FromJson<InventoryChange>(netMsg.ReadMessage<StringMessage>().value);
        ApplyChanges(changes);

        if (_playerController.HasMenuOpen && GameManager.Instance.MainCanvasObjects.CraftingUi.activeSelf)
        {
            var uiScript = GameManager.Instance.MainCanvasObjects.CraftingUi.GetComponent<CraftingUi>();
            uiScript.ResetUi();
            uiScript.LoadInventory();
        }
    }

    private void ApplyChanges(Assets.Scripts.Data.Inventory changes)
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
        var addedItemsCount = addedItems.Count();

        Items.AddRange(addedItems);

        if (addedItemsCount == 1)
        {
            var addedItem = addedItems.First();

            //todo: make this a slide-out alert instead
            Debug.Log($"{addedItem.GetFullName()} was added");
        }
        else
        {
            //todo: make this a slide-out alert instead
            Debug.Log($"Added {addedItemsCount} items to the inventory after handling message on " + (isServer ? "server" : "client") + " for " + gameObject.name);
        }

        if (changes.EquipSlots.Length == EquippedItems.Length)
        {
            EquippedItems = changes.EquipSlots;
        }
        else
        {
            //todo:
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

        var foo = "";
    }

}
