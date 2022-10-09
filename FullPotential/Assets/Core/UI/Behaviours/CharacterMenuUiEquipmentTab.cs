using System;
using System.Linq;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Gameplay.Tooltips;
using FullPotential.Core.PlayerBehaviours;
using FullPotential.Core.Ui.Components;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Ui.Behaviours
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
        private ResultFactory _resultFactory;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerState = GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerState>();
            _resultFactory = GameManager.Instance.GetService<ResultFactory>();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            ResetEquipmentUi(true);
        }

        // ReSharper disable once UnusedMember.Global
        public void OnSlotClick(GameObject clickedObject)
        {
            if (_lastClickedSlot == clickedObject)
            {
                ResetEquipmentUi();
                return;
            }

            switch (clickedObject.name)
            {
                case nameof(SlotGameObjectName.Helm): LoadInventoryItems(clickedObject, IGear.GearCategory.Helm); break;
                case nameof(SlotGameObjectName.Chest): LoadInventoryItems(clickedObject, IGear.GearCategory.Chest); break;
                case nameof(SlotGameObjectName.Legs): LoadInventoryItems(clickedObject, IGear.GearCategory.Legs); break;
                case nameof(SlotGameObjectName.Feet): LoadInventoryItems(clickedObject, IGear.GearCategory.Feet); break;
                case nameof(SlotGameObjectName.Barrier): LoadInventoryItems(clickedObject, IGear.GearCategory.Barrier); break;

                case nameof(SlotGameObjectName.LeftHand):
                case nameof(SlotGameObjectName.RightHand): LoadInventoryItems(clickedObject, IGear.GearCategory.Hand); break;

                case nameof(SlotGameObjectName.LeftRing):
                case nameof(SlotGameObjectName.RightRing): LoadInventoryItems(clickedObject, IGear.GearCategory.Ring); break;

                case nameof(SlotGameObjectName.Amulet): LoadInventoryItems(clickedObject, IGear.GearCategory.Amulet); break;
                case nameof(SlotGameObjectName.Belt): LoadInventoryItems(clickedObject, IGear.GearCategory.Belt); break;

                default:
                    Debug.LogError($"Cannot handle click for slot {clickedObject.name}");
                    return;
            }

            _lastClickedSlot = clickedObject;
        }

        private void SetSlot(GameObject slot, ItemBase item)
        {
            //todo: is there a way of avoiding using .Find() ?
            var slotImage = slot.transform.Find("Image").GetComponent<Image>();

            slotImage.color = item != null ? Color.grey : Color.clear;

            var tooltip = slot.GetComponent<Tooltip>();
            if (tooltip != null)
            {
                tooltip.ClearHandlers();

                // ReSharper disable once UnusedParameter.Local
                tooltip.OnPointerEnterForTooltip += pointerEventData =>
                {
                    Tooltips.ShowTooltip(_resultFactory.GetItemDescription(item));
                };
            }
        }

        private GameObject GetSlotGameObject(string slotName)
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
            _componentsContainer.transform.DestroyChildren();
            _lastClickedSlot = null;

            if (reloadSlots)
            {
                foreach (SlotGameObjectName slotGameObjectName in Enum.GetValues(typeof(SlotGameObjectName)))
                {
                    var slotName = Enum.GetName(typeof(SlotGameObjectName), slotGameObjectName);
                    var item = _playerState.Inventory.GetItemInSlot(slotGameObjectName);

                    SetSlot(GetSlotGameObject(slotName), item);

                    if (item is Weapon weapon && weapon.IsTwoHanded)
                    {
                        var otherSlotName = slotGameObjectName == SlotGameObjectName.LeftHand
                            ? SlotGameObjectName.RightHand.ToString()
                            : SlotGameObjectName.LeftHand.ToString();
                        SetSlot(GetSlotGameObject(otherSlotName), null);
                    }
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

                if (!Enum.TryParse<SlotGameObjectName>(slot.name, out var slotGameObjectName))
                {
                    Debug.LogError($"Failed to find slot for name {slot.name}");
                    return;
                }

                var playerInventory = (PlayerInventory)_playerState.Inventory;

                var changeResult = playerInventory.HandleSlotChange(item, slotGameObjectName);
                playerInventory.SpawnEquippedObject(changeResult.WasEquipped ? item : null, slotGameObjectName);
                foreach (var slotString in changeResult.SlotsToSend.Where(x => x != slotGameObjectName.ToString()))
                {
                    playerInventory.SpawnEquippedObject(null, Enum.Parse<SlotGameObjectName>(slotString));
                }
                ResetEquipmentUi(true);

                playerInventory.EquipItemServerRpc(item.Id, slotGameObjectName);
            });
        }

    }
}
