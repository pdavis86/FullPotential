using FullPotential.Assets.Api.Registry;
using FullPotential.Assets.Core.Extensions;
using FullPotential.Assets.Core.Registry.Base;
using FullPotential.Assets.Core.Storage;
using System;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

public class CharacterMenuUiEquipmentTab : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject _componentsContainer;
    [SerializeField] private GameObject _lhs;
    [SerializeField] private GameObject _rhs;
    [SerializeField] private GameObject _inventoryRowPrefab;
#pragma warning restore 0649

    private GameObject _lastClickedSlot;
    private PlayerState _playerState;
    private PlayerClientSide _playerClientSide;

    private void Awake()
    {
        _playerState = GameManager.Instance.DataStore.LocalPlayer.GetComponent<PlayerState>();
        _playerClientSide = _playerState.gameObject.GetComponent<PlayerClientSide>();
    }

    private void OnEnable()
    {
        ResetEquipmentUi(true);
    }

    public void OnSlotClick(GameObject clickedObject)
    {
        if (_lastClickedSlot == clickedObject)
        {
            ResetEquipmentUi();
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

    private void SetSlot(GameObject slot, ItemBase item)
    {
        //Debug.LogError($"Setting slot '{slot?.name}' to '{item?.Id}'");

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
        var leftAttempt = _lhs.transform.Find(slotName);
        if (leftAttempt != null)
        {
            return leftAttempt.gameObject;
        }

        var rightAttempt = _rhs.transform.Find(slotName);
        if (rightAttempt != null)
        {
            return rightAttempt.gameObject;
        }

        Debug.LogError($"Failed to find slot {slotName}");
        return null;
    }

    private void ResetEquipmentUi(bool reloadSlots = false)
    {
        _componentsContainer.SetActive(false);
        _componentsContainer.transform.Clear();
        _lastClickedSlot = null;

        if (reloadSlots)
        {
            for (var i = 0; i < _playerState.Inventory.GetSlotCount(); i++)
            {
                var slotName = Enum.GetName(typeof(PlayerInventory.SlotIndexToGameObjectName), i);
                SetSlot(GetSlot(slotName), _playerState.Inventory.GetItemInSlot(i));
            }
        }
    }

    private void LoadInventoryItems(GameObject slot, IGear.GearSlot? inventorySlot = null)
    {
        _componentsContainer.SetActive(true);

        InventoryItemsList.LoadInventoryItems(
            slot,
            _componentsContainer,
            _inventoryRowPrefab,
            _playerState.Inventory,
            HandleRowToggle,
            inventorySlot
        );
    }

    private void HandleRowToggle(GameObject row, GameObject slot, ItemBase item)
    {
        var toggle = row.GetComponent<Toggle>();

        // ReSharper disable once UnusedParameter.Local
        toggle.onValueChanged.AddListener(isOn =>
        {
            Tooltips.HideTooltip();

            if (!Enum.TryParse<PlayerInventory.SlotIndexToGameObjectName>(slot.name, out var slotResult))
            {
                Debug.LogError($"Failed to find slot for name {slot.name}");
                return;
            }

            _playerState.Inventory.EquipItem(item.Id, (int)slotResult, true);

            _playerClientSide.ChangeEquipsServerRpc(_playerState.Inventory.EquipSlots);

            ResetEquipmentUi(true);
        });
    }

}
