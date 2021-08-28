using FullPotential.Assets.Api.Registry;
using FullPotential.Assets.Core.Extensions;
using FullPotential.Assets.Core.Registry.Base;
using FullPotential.Assets.Core.Storage;
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

        var itemsForSlot = inventory.GetCompatibleItemsForSlot(inventorySlot);

        if (!itemsForSlot.Any())
        {
            Debug.LogWarning("There are no items of the correct type");
            return;
        }

        foreach (var item in itemsForSlot)
        {
            var isEquipped = inventory.IsEquipped(item.Id);

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
            tooltip.ClearHandlers();
            // ReSharper disable once UnusedParameter.Local
            tooltip.OnPointerEnterForTooltip += pointerEventData =>
            {
                Tooltips.ShowTooltip(GameManager.Instance.ResultFactory.GetItemDescription(item, false));
            };

            rowCounter++;
        }

        const int spacer = 5;
        var containerRectTrans = componentsContainer.GetComponent<RectTransform>();
        containerRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowCounter * (rowRectTransform.rect.height + spacer));
    }

}
