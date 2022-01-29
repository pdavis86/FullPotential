using FullPotential.Api.Registry;
using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Behaviours.Ui.Components;
using FullPotential.Core.Behaviours.UtilityBehaviours;
using FullPotential.Core.Extensions;
using FullPotential.Core.Registry.Base;
using System;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.Ui
{
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

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerState = GameManager.Instance.LocalGameDataStore.GameObject.GetComponent<PlayerState>();
        }

        // ReSharper disable once UnusedMember.Local
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
                case nameof(PlayerInventory.SlotGameObjectName.Helm): LoadInventoryItems(clickedObject, IGear.GearCategory.Helm); break;
                case nameof(PlayerInventory.SlotGameObjectName.Chest): LoadInventoryItems(clickedObject, IGear.GearCategory.Chest); break;
                case nameof(PlayerInventory.SlotGameObjectName.Legs): LoadInventoryItems(clickedObject, IGear.GearCategory.Legs); break;
                case nameof(PlayerInventory.SlotGameObjectName.Feet): LoadInventoryItems(clickedObject, IGear.GearCategory.Feet); break;
                case nameof(PlayerInventory.SlotGameObjectName.Barrier): LoadInventoryItems(clickedObject, IGear.GearCategory.Barrier); break;

                case nameof(PlayerInventory.SlotGameObjectName.LeftHand):
                case nameof(PlayerInventory.SlotGameObjectName.RightHand): LoadInventoryItems(clickedObject, IGear.GearCategory.Hand); break;

                case nameof(PlayerInventory.SlotGameObjectName.LeftRing):
                case nameof(PlayerInventory.SlotGameObjectName.RightRing): LoadInventoryItems(clickedObject, IGear.GearCategory.Ring); break;

                case nameof(PlayerInventory.SlotGameObjectName.Amulet): LoadInventoryItems(clickedObject, IGear.GearCategory.Amulet); break;
                case nameof(PlayerInventory.SlotGameObjectName.Belt): LoadInventoryItems(clickedObject, IGear.GearCategory.Belt); break;

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

            var rightAttempt = _rhs.FindInDescendants(slotName);
            if (rightAttempt != null)
            {
                return rightAttempt.gameObject;
            }

            Debug.LogError($"Failed to find slot {slotName}");
            return null;
        }

        public void ResetEquipmentUi(bool reloadSlots = false)
        {
            _componentsContainer.SetActive(false);
            _componentsContainer.transform.Clear();
            _lastClickedSlot = null;

            if (reloadSlots)
            {
                foreach (PlayerInventory.SlotGameObjectName slotGameObjectName in Enum.GetValues(typeof(PlayerInventory.SlotGameObjectName)))
                {
                    var slotName = Enum.GetName(typeof(PlayerInventory.SlotGameObjectName), slotGameObjectName);
                    SetSlot(GetSlot(slotName), _playerState.Inventory.GetItemInSlot(slotGameObjectName));
                }
            }
        }

        private void LoadInventoryItems(GameObject slot, IGear.GearCategory? gearCategory = null)
        {
            _componentsContainer.SetActive(true);

            InventoryItemsList.LoadInventoryItems(
                slot,
                _componentsContainer,
                _inventoryRowPrefab,
                _playerState.Inventory,
                HandleRowToggle,
                gearCategory
            );
        }

        private void HandleRowToggle(GameObject row, GameObject slot, ItemBase item)
        {
            var toggle = row.GetComponent<Toggle>();

            // ReSharper disable once UnusedParameter.Local
            toggle.onValueChanged.AddListener(isOn =>
            {
                Tooltips.HideTooltip();

                if (!Enum.TryParse<PlayerInventory.SlotGameObjectName>(slot.name, out var slotResult))
                {
                    Debug.LogError($"Failed to find slot for name {slot.name}");
                    return;
                }

                _playerState.Inventory.EquipItemServerRpc(item.Id, slotResult);
            });
        }

    }
}
