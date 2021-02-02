using Assets.Scripts.Attributes;
using Assets.Scripts.Crafting.Results;
using System.Collections.Generic;
using UnityEngine;

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

    [ClientSideOnlyTemp]
    public void LoadFrom(string dataFromServer)
    {
        //todo: 
    }

    public bool Add(ItemBase item)
    {
        if (Items.Count == MaxItems)
        {
            //todo: send to storage instead
            return false;
        }

        Items.Add(item);
        return true;
    }

    public bool Remove(ItemBase item)
    {
        return Items.Remove(item);
    }

}
