using FullPotential.Assets.Api.Registry;
using FullPotential.Assets.Core.Extensions;
using FullPotential.Assets.Core.Registry.Base;
using FullPotential.Assets.Core.Storage;
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
#pragma warning disable 0649
    [SerializeField] private GameObject _componentsContainer;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private GameObject _slotsContainer;
#pragma warning restore 0649

    private PlayerState _playerState;
    private GameObject _lastClickedSlot;

    private void Awake()
    {
        _playerState = GameManager.Instance.DataStore.LocalPlayer.GetComponent<PlayerState>();
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
            // ReSharper disable once UnusedParameter.Local
            tooltip.OnPointerEnterForTooltip += pointerEventData =>
            {
                Tooltips.ShowTooltip(GameManager.Instance.ResultFactory.GetItemDescription(item));
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
            for (var i = 0; i < _playerState.Inventory.GetSlotCount(); i++)
            {
                var slotName = System.Enum.GetName(typeof(PlayerInventory.SlotIndexToGameObjectName), i);
                SetSlot(GetSlot(slotName), _playerState.Inventory.GetItemInSlot(i));
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
            case nameof(PlayerInventory.SlotIndexToGameObjectName.Helm): LoadInventoryItems(clickedObject, IGear.GearSlot.Helm); break;
            case nameof(PlayerInventory.SlotIndexToGameObjectName.Chest): LoadInventoryItems(clickedObject, IGear.GearSlot.Chest); break;
            case nameof(PlayerInventory.SlotIndexToGameObjectName.Legs): LoadInventoryItems(clickedObject, IGear.GearSlot.Legs); break;
            case nameof(PlayerInventory.SlotIndexToGameObjectName.Feet): LoadInventoryItems(clickedObject, IGear.GearSlot.Feet); break;
            case nameof(PlayerInventory.SlotIndexToGameObjectName.Barrier): LoadInventoryItems(clickedObject, IGear.GearSlot.Barrier); break;

            case nameof(PlayerInventory.SlotIndexToGameObjectName.LeftHand):
            case nameof(PlayerInventory.SlotIndexToGameObjectName.RightHand): LoadInventoryItems(clickedObject, IGear.GearSlot.Hand); break;

            case nameof(PlayerInventory.SlotIndexToGameObjectName.LeftRing):
            case nameof(PlayerInventory.SlotIndexToGameObjectName.RightRing): LoadInventoryItems(clickedObject, IGear.GearSlot.Ring); break;

            case nameof(PlayerInventory.SlotIndexToGameObjectName.Amulet): LoadInventoryItems(clickedObject, IGear.GearSlot.Amulet); break;
            case nameof(PlayerInventory.SlotIndexToGameObjectName.Belt): LoadInventoryItems(clickedObject, IGear.GearSlot.Belt); break;

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
            _playerState.Inventory,
            HandleRowToggle,
            inventorySlot
        );
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

                _playerState.Inventory.SetItemToSlot(slot.name, item.Id);

                SetSlot(slot, item);

                ResetUi(true);
            }
        });
    }

}
