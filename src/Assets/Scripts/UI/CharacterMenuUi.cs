using Assets.ApiScripts.Crafting;
using Assets.Core.Crafting;
using Assets.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable PossibleMultipleEnumeration

public class CharacterMenuUi : MonoBehaviour
{
    [SerializeField] private GameObject _componentsContainer;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private GameObject _slotsContainer;

    private Inventory _inventory;
    private GameObject _lastClickedSlot;

    private void Awake()
    {
        _inventory = GameManager.Instance.LocalPlayer.GetComponent<Inventory>();
    }

    private void OnEnable()
    {
        ResetUi(true);
    }

    private void SetSlot(GameObject slot, ItemBase item)
    {
        //todo: set to image of the item selected instead
        var slotImage = slot.transform.Find("Image").GetComponent<Image>();
        slotImage.color = item != null ? Color.white : Color.clear;

        var tooltip = slot.GetComponent<Tooltip>();
        if (tooltip != null)
        {
            tooltip.ClearHandlers();
            tooltip.OnPointerEnterForTooltip += pointerEventData =>
            {
                Tooltips.ShowTooltip(ResultFactory.GetItemDescription(item));
            };
        }
    }

    private GameObject GetSlot(string slotName)
    {
        var lhs = _slotsContainer.transform.Find("LHS");
        var leftAttempt = lhs.Find(slotName);
        if (leftAttempt != null)
        {
            return leftAttempt.gameObject;
        }

        var rhs = _slotsContainer.transform.Find("RHS");
        var rightAttempt = rhs.Find(slotName);
        if (rightAttempt != null)
        {
            return rightAttempt.gameObject;
        }

        Debug.LogError($"Failed to find slot {slotName}");
        return null;
    }

    public void ResetUi(bool reloadSlots = false)
    {
        _componentsContainer.SetActive(false);
        _componentsContainer.transform.Clear();
        _lastClickedSlot = null;

        if (reloadSlots)
        {
            for (var i = 0; i < _inventory.EquipSlots.Length; i++)
            {
                var slotName = System.Enum.GetName(typeof(Inventory.SlotIndexToGameObjectName), i);

                var itemId = _inventory.EquipSlots[i];
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    var item = _inventory.Items.FirstOrDefault(x => x.Id == itemId);

                    //Debug.Log($"Displaying '{itemId}' in UI slot '{slotName}'");

                    SetSlot(GetSlot(slotName), item);
                }
                else
                {
                    SetSlot(GetSlot(slotName), null);
                }
            }
        }
    }

    public void OnSlotClick(GameObject clickedObject)
    {
        if (_lastClickedSlot == clickedObject)
        {
            ResetUi();
            return;
        }

        switch (clickedObject.name)
        {
            case nameof(Inventory.SlotIndexToGameObjectName.Helm): LoadInventoryItems(clickedObject, IGear.GearSlot.Helm); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Chest): LoadInventoryItems(clickedObject, IGear.GearSlot.Chest); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Legs): LoadInventoryItems(clickedObject, IGear.GearSlot.Legs); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Feet): LoadInventoryItems(clickedObject, IGear.GearSlot.Feet); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Barrier): LoadInventoryItems(clickedObject, IGear.GearSlot.Barrier); break;

            case nameof(Inventory.SlotIndexToGameObjectName.LeftHand):
            case nameof(Inventory.SlotIndexToGameObjectName.RightHand): LoadInventoryItems(clickedObject, IGear.GearSlot.Hand); break;

            case nameof(Inventory.SlotIndexToGameObjectName.LeftRing):
            case nameof(Inventory.SlotIndexToGameObjectName.RightRing): LoadInventoryItems(clickedObject, IGear.GearSlot.Ring); break;

            case nameof(Inventory.SlotIndexToGameObjectName.Amulet): LoadInventoryItems(clickedObject, IGear.GearSlot.Amulet); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Belt): LoadInventoryItems(clickedObject, IGear.GearSlot.Belt); break;

            default:
                Debug.LogError($"Cannot handle click for slot {clickedObject.name}");
                return;
        }

        _lastClickedSlot = clickedObject;
    }

    private void LoadInventoryItems(GameObject slot, IGear.GearSlot? inventorySlot = null)
    {
        _componentsContainer.SetActive(true);

        InventoryItemsList.LoadInventoryItems(
            slot,
            _componentsContainer,
            _rowPrefab,
            _inventory,
            HandleRowToggle,
            inventorySlot,
            true
        );

        //_componentsContainer.transform.Clear();

        //var rowRectTransform = _rowPrefab.GetComponent<RectTransform>();
        //var rowCounter = 0;

        //var itemsOfTypes = _inventory.Items.Where(x =>
        //{
        //    var itemType = x.GetType();

        //    if (!string.IsNullOrWhiteSpace(gearSubType) && x is GearBase gearItem)
        //    {
        //        return gearItem.SubType == gearSubType;
        //    }
        //    else
        //    {
        //        return itemTypes.Contains(itemType);
        //    }
        //});

        //if (!itemsOfTypes.Any())
        //{
        //    Debug.LogWarning("There are no items of the correct type");
        //    return;
        //}

        //foreach (var item in itemsOfTypes)
        //{
        //    var row = Instantiate(_rowPrefab, _componentsContainer.transform);
        //    row.transform.Find("ItemName").GetComponent<Text>().text = item.GetFullName();

        //    var rowImage = row.GetComponent<Image>();
        //    if (_inventory.EquipSlots.Contains(item.Id))
        //    {
        //        rowImage.color = Color.green;
        //    }

        //    var toggle = row.GetComponent<Toggle>();
        //    toggle.onValueChanged.AddListener(isOn =>
        //    {
        //        if (isOn)
        //        {
        //            Tooltips.HideTooltip();

        //            //Debug.Log($"Setting item for slot '{slot.name}' to be '{item.Name}'");

        //            _inventory.SetItemToSlotOnBoth(slot.name, item.Id);

        //            SetSlot(slot, item);

        //            ResetUi(true);
        //        }
        //    });

        //    var tooltip = row.GetComponent<Tooltip>();
        //    tooltip.OnPointerEnterForTooltip += pointerEventData =>
        //    {
        //        Tooltips.ShowTooltip(ResultFactory.GetItemDescription(item, false));
        //    };

        //    rowCounter++;
        //}

        //const int spacer = 5;
        //var containerRectTrans = _componentsContainer.GetComponent<RectTransform>();
        //containerRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowCounter * (rowRectTransform.rect.height + spacer));
    }

    private void HandleRowToggle(GameObject row, GameObject slot, ItemBase item)
    {
        var toggle = row.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                Tooltips.HideTooltip();

                //Debug.Log($"Setting item for slot '{slot.name}' to be '{item.Name}'");

                _inventory.SetItemToSlotOnBoth(slot.name, item.Id);

                SetSlot(slot, item);

                ResetUi(true);
            }
        });
    }

}
