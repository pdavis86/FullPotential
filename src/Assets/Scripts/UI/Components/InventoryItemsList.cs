using Assets.Core.Crafting;
using Assets.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemsList : MonoBehaviour
{
    public static void LoadInventoryItems(
        GameObject slot,
        GameObject componentsContainer,
        GameObject rowPrefab,
        Inventory inventory,
        Action<GameObject, GameObject, ItemBase> toggleAction,
        IEnumerable<System.Type> itemTypes,
        string gearSubType = null,
        bool showEquippedItems = true
    )
    {
        componentsContainer.SetActive(true);
        componentsContainer.transform.Clear();

        var rowRectTransform = rowPrefab.GetComponent<RectTransform>();
        var rowCounter = 0;

        var itemsOfTypes = inventory.Items.Where(x =>
        {
            var itemType = x.GetType();

            if (!string.IsNullOrWhiteSpace(gearSubType) && x is GearBase gearItem)
            {
                return gearItem.SubType == gearSubType;
            }
            else
            {
                return itemTypes == null || !itemTypes.Any() || itemTypes.Contains(itemType);
            }
        });

        if (!itemsOfTypes.Any())
        {
            Debug.LogWarning("There are no items of the correct type");
            return;
        }

        foreach (var item in itemsOfTypes)
        {
            var isEquipped = inventory.EquipSlots.Contains(item.Id);

            if (isEquipped && !showEquippedItems)
            {
                continue;
            }

            var row = Instantiate(rowPrefab, componentsContainer.transform);
            row.transform.Find("ItemName").GetComponent<Text>().text = item.GetFullName();

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
