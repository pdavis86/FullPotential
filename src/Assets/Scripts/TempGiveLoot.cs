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

    public override void InteractWith()
    {
        Inventory.Add(GameManager.Instance.ResultFactory.GetLootDrop());

        //todo: comment out
        Debug.Log($"Inventory now has {Inventory.Items.Count} items in it");
    }
}
