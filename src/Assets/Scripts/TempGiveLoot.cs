using Assets.Scripts.Crafting.Results;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class TempGiveLoot : Interactable
{
    public Inventory Inventory;

    private readonly ResultFactory _resultFactory = new ResultFactory();

    public override void InteractWith()
    {
        Inventory.Add(_resultFactory.GetLootDrop());
        Debug.Log($"Inventory now has {Inventory.Items.Count} items in it");
    }
}
