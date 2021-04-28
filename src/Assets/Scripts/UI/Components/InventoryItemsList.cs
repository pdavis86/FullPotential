using Assets.ApiScripts.Registry;
using Assets.Core.Crafting;
using Assets.Core.Extensions;
using Assets.Core.Registry.Base;
using Assets.Core.Registry.Types;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable PossibleMultipleEnumeration

public class InventoryItemsList : MonoBehaviour
{
    public static void LoadInventoryItems(
        GameObject slot,
        GameObject componentsContainer,
        GameObject rowPrefab,
        PlayerInventory inventory,
        Action<GameObject, GameObject, ItemBase> toggleAction,
        IGear.GearSlot? inventorySlot = null,
        bool showEquippedItems = true
    )
    {
        componentsContainer.SetActive(true);
        componentsContainer.transform.Clear();

        var rowRectTransform = rowPrefab.GetComponent<RectTransform>();
        var rowCounter = 0;

        var itemsForSlot = inventory.Items.Where(x =>
            inventorySlot == null
            || (x is Accessory acc && (int)((IGearAccessory)acc.RegistryType).InventorySlot == (int)inventorySlot)
            || (x is Armor armor && (int)((IGearArmor)armor.RegistryType).InventorySlot == (int)inventorySlot)
            || ((x is Weapon || x is Spell) && inventorySlot == IGear.GearSlot.Hand)
        );

        if (!itemsForSlot.Any())
        {
            Debug.LogWarning("There are no items of the correct type");
            return;
        }

        foreach (var item in itemsForSlot)
        {
            var isEquipped = inventory.EquipSlots.Contains(item.Id);

            if (isEquipped && !showEquippedItems)
            {
                continue;
            }

            var row = Instantiate(rowPrefab, componentsContainer.transform);
            row.transform.Find("ItemName").GetComponent<Text>().text = item.Name;

            if (isEquipped)
            {
                var rowImage = row.GetComponent<Image>();
                rowImage.color = Color.green;
            }

            toggleAction(row, slot, item);

            var tooltip = row.GetComponent<Tooltip>();
            tooltip.OnPointerEnterForTooltip += pointerEventData =>
            {
                Tooltips.ShowTooltip(ResultFactory.GetItemDescription(item, false));
            };

            rowCounter++;
        }

        const int spacer = 5;
        var containerRectTrans = componentsContainer.GetComponent<RectTransform>();
        containerRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowCounter * (rowRectTransform.rect.height + spacer));
    }

}
