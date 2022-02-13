using System;
using System.Linq;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Utilities.UtilityBehaviours;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.UiBehaviours.Components
{
    public class InventoryItemsList : MonoBehaviour
    {
        public static void LoadInventoryItems(
            GameObject slot,
            GameObject componentsContainer,
            GameObject rowPrefab,
            IPlayerInventory playerInventory,
            Action<GameObject, GameObject, ItemBase> toggleAction,
            IGear.GearCategory? gearCategory = null,
            bool showEquippedItems = true
        )
        {
            componentsContainer.SetActive(true);
            componentsContainer.transform.DestroyChildren();

            var rowRectTransform = rowPrefab.GetComponent<RectTransform>();
            var rowCounter = 0;

            var itemsForSlot = playerInventory.GetCompatibleItemsForSlot(gearCategory);

            if (!itemsForSlot.Any())
            {
                Debug.LogWarning("There are no items of the correct type");
                return;
            }

            foreach (var item in itemsForSlot)
            {
                var isEquipped = playerInventory.GetEquippedWithItemId(item.Id) != null;

                if (isEquipped && !showEquippedItems)
                {
                    continue;
                }

                var row = Instantiate(rowPrefab, componentsContainer.transform);
                row.transform.Find("ItemName").GetComponent<Text>().text = item.Name;

                if (isEquipped)
                {
                    var rowToggle = row.GetComponent<Toggle>();
                    rowToggle.isOn = true;

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
}
