using Assets.Scripts.Crafting.Results;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class CharacterMenuUi : MonoBehaviour
{
    [SerializeField] private GameObject _componentsContainer;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private GameObject _slotsContainer;

    private Inventory _inventory;
    private GameObject _activeSlot;

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
        slotImage.sprite = null;

        var tooltip = slot.GetComponent<CraftingTooltip>();
        if (tooltip != null)
        {
            tooltip.ClearHandlers();
            tooltip.OnPointerEnterForTooltip += pointerEventData =>
            {
                Tooltips.ShowTooltip(CraftingUi.GetItemDescription(item));
            };
        }
    }

    private GameObject GetSlot(string name)
    {
        //todo: do this better

        var lhs = _slotsContainer.transform.Find("LHS");
        var leftAttempt = lhs.Find(name);
        if (leftAttempt != null)
        {
            return leftAttempt.gameObject;
        }

        var rhs = _slotsContainer.transform.Find("RHS");
        var rightAttempt = rhs.Find(name);
        if (rightAttempt != null)
        {
            return rightAttempt.gameObject;
        }

        Debug.Log($"Failed to find slot {name}");
        return null;
    }

    public void ResetUi(bool reloadSlots = false)
    {
        _componentsContainer.SetActive(false);
        _componentsContainer.transform.Clear();
        _activeSlot = null;

        if (reloadSlots)
        {
            for (var i = 0; i < _inventory.EquipSlots.Length; i++)
            {
                var itemId = _inventory.EquipSlots[i];
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    var slotName = System.Enum.GetName(typeof(Inventory.SlotIndexToGameObjectName), i);
                    //Debug.Log($"Displaying '{itemId}' in UI slot '{slotName}'");
                    SetSlot(GetSlot(slotName), _inventory.Items.First(x => x.Id == itemId));
                }
            }
        }
    }

    public void OnSlotClick(GameObject clickedObject)
    {
        if (_activeSlot == clickedObject)
        {
            ResetUi();
            return;
        }

        switch (clickedObject.name)
        {
            case nameof(Inventory.SlotIndexToGameObjectName.Helm): LoadInventoryItems(new[] { typeof(Armor) }, Armor.Helm); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Chest): LoadInventoryItems(new[] { typeof(Armor) }, Armor.Chest); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Legs): LoadInventoryItems(new[] { typeof(Armor) }, Armor.Legs); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Feet): LoadInventoryItems(new[] { typeof(Armor) }, Armor.Feet); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Barrier): LoadInventoryItems(new[] { typeof(Armor) }, Armor.Barrier); break;

            case nameof(Inventory.SlotIndexToGameObjectName.LeftHand):
            case nameof(Inventory.SlotIndexToGameObjectName.RightHand): LoadInventoryItems(new[] { typeof(Weapon), typeof(Spell) }); break;

            case nameof(Inventory.SlotIndexToGameObjectName.LeftRing):
            case nameof(Inventory.SlotIndexToGameObjectName.RightRing): LoadInventoryItems(new[] { typeof(Accessory) }, Accessory.Ring); break;

            case nameof(Inventory.SlotIndexToGameObjectName.Gloves): LoadInventoryItems(new[] { typeof(Accessory) }, Accessory.Gloves); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Amulet): LoadInventoryItems(new[] { typeof(Accessory) }, Accessory.Amulet); break;
            case nameof(Inventory.SlotIndexToGameObjectName.Belt): LoadInventoryItems(new[] { typeof(Accessory) }, Accessory.Belt); break;

            default:
                Debug.LogError($"Cannot handle click for slot {clickedObject.name}");
                return;
        }

        _activeSlot = clickedObject;
    }

    private void LoadInventoryItems(IEnumerable<System.Type> itemTypes, string gearSubType = null)
    {
        //todo: once this is working make the gameobjects and code code common with crafting ui

        _componentsContainer.SetActive(true);

        _componentsContainer.transform.Clear();

        var rowRectTransform = _rowPrefab.GetComponent<RectTransform>();
        var rowCounter = 0;

        var itemsOfTypes = _inventory.Items.Where(x =>
        {
            var itemType = x.GetType();

            if (!string.IsNullOrWhiteSpace(gearSubType) && x is GearBase gearItem)
            {
                return !_inventory.EquipSlots.Contains(x.Id) && gearItem.SubType == gearSubType;
            }
            else
            {
                return !_inventory.EquipSlots.Contains(x.Id) && itemTypes.Contains(itemType);
            }
        });

        if (itemsOfTypes.Count() == 0)
        {
            Debug.LogWarning("There are no items of the correct type");
            return;
        }

        foreach (var item in itemsOfTypes)
        {
            var row = Instantiate(_rowPrefab, _componentsContainer.transform);
            row.transform.Find("ItemName").GetComponent<Text>().text = item.GetFullName();

            var toggle = row.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    Tooltips.HideTooltip();

                    //Debug.Log($"Setting item for slot '{_activeSlot.name}' to be '{item.Name}'");

                    _inventory.SetItemToSlotOnBoth(_activeSlot.name, item.Id);

                    SetSlot(_activeSlot, item);

                    ResetUi();
                }
            });

            var tooltip = row.GetComponent<CraftingTooltip>();
            tooltip.OnPointerEnterForTooltip += pointerEventData =>
            {
                Tooltips.ShowTooltip(CraftingUi.GetItemDescription(item, false));
            };

            rowCounter++;
        }

        const int spacer = 5;
        var containerRectTrans = _componentsContainer.GetComponent<RectTransform>();
        containerRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowCounter * (rowRectTransform.rect.height + spacer));
    }

}
