using Assets.Scripts.Crafting.Results;
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

public class Inventory : MonoBehaviour
{
    public int MaxItems = 10;


    [HideInInspector]
    public List<ItemBase> Items { get; private set; }

    private void Awake()
    {
        Items = new List<ItemBase>();
    }

    private void Start()
    {
        if (NetworkClient.active)
        {
            var netId = GetComponent<NetworkIdentity>();
            if (netId == null)
            {
                return;
            }
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryLoad, OnLoadInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddItem, OnAddItemToInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddAccessory, OnAddAccessoryToInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddArmor, OnAddArmorToInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddSpell, OnAddSpellToInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddWeapon, OnAddWeaponToInventory);
        }
    }

    private void OnLoadInventory(NetworkMessage netMsg)
    {
        //todo: load inv from save
    }

    private void OnAddItemToInventory(NetworkMessage netMsg)
    {
        AddOfType<ItemBase>(netMsg.ReadMessage<StringMessage>().value);
    }

    private void OnAddAccessoryToInventory(NetworkMessage netMsg)
    {
        AddOfType<Accessory>(netMsg.ReadMessage<StringMessage>().value);
    }

    private void OnAddArmorToInventory(NetworkMessage netMsg)
    {
        AddOfType<Armor>(netMsg.ReadMessage<StringMessage>().value);
    }

    private void OnAddSpellToInventory(NetworkMessage netMsg)
    {
        AddOfType<Spell>(netMsg.ReadMessage<StringMessage>().value);
    }

    private void OnAddWeaponToInventory(NetworkMessage netMsg)
    {
        AddOfType<Weapon>(netMsg.ReadMessage<StringMessage>().value);
    }







    private void AddOfType<T>(string stringValue) where T : ItemBase
    {
        Add(JsonUtility.FromJson<T>(stringValue));
        Debug.Log($"Inventory now has {Items.Count} items in it");
    }

    public void Add(ItemBase item)
    {
        if (Items.Count == MaxItems)
        {
            //todo: send to storage instead
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

}
