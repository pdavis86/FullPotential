using System;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Tooltips;
using FullPotential.Core.Player;
using FullPotential.Core.UI.Behaviours;
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
        private ILocalizer _localizer;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            _playerState = GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerState>();
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

            //todo: Implement UI slot creation then replace hard-coded values

            switch (clickedObject.name)
            {
                case nameof(SlotGameObjectName.Helm): LoadInventoryItems(clickedObject, new Guid("b1b9b067-2523-4d57-a4c1-14b3a623f5f3")); break;
                case nameof(SlotGameObjectName.Chest): LoadInventoryItems(clickedObject, new Guid("a2989e18-6830-4770-8695-1c8592137e2d")); break;
                case nameof(SlotGameObjectName.Legs): LoadInventoryItems(clickedObject, new Guid("4eda8bc2-6929-4ad6-a5e1-3103b2cbcdac")); break;
                case nameof(SlotGameObjectName.Feet): LoadInventoryItems(clickedObject, new Guid("e42aefc3-2834-4f61-897f-5fb62d439b56")); break;

                case nameof(SlotGameObjectName.LeftHand):
                case nameof(SlotGameObjectName.RightHand): LoadInventoryItems(clickedObject, Guid.Empty); break;

                case nameof(SlotGameObjectName.LeftRing):
                case nameof(SlotGameObjectName.RightRing): LoadInventoryItems(clickedObject, new Guid("c70a9495-0ef7-48fb-9b16-aad5fe7b29ad")); break;

                case nameof(SlotGameObjectName.Amulet): LoadInventoryItems(clickedObject, new Guid("e02761e5-5155-4b61-8f1c-8feb240a420c")); break;
                case nameof(SlotGameObjectName.Belt): LoadInventoryItems(clickedObject, new Guid("3275abb7-fc48-4682-abc1-85dda7abf24e")); break;
                case nameof(SlotGameObjectName.Barrier): LoadInventoryItems(clickedObject, new Guid("c2dbfd42-9a5b-4b0b-ba90-6b02ab710859")); break;
                case nameof(SlotGameObjectName.Reloader): LoadInventoryItems(clickedObject, new Guid("0298c98c-d9db-4127-bd57-e3045340088f")); break;

                default:
                    Debug.LogError($"Cannot handle click for slot {clickedObject.name}");
                    return;
            }

            _lastClickedSlot = clickedObject;
        }

        private void SetSlot(GameObject slot, ItemBase item)
        {
            var slotImage = slot.GetComponent<InventoryUiSlot>().Image;

            slotImage.color = item != null ? Color.grey : Color.clear;

            var tooltip = slot.GetComponent<Tooltip>();
            if (tooltip != null)
            {
                tooltip.ClearHandlers();

                if (item != null)
                {
                    tooltip.OnPointerEnterForTooltip += _ =>
                    {
                        Tooltips.ShowTooltip(item.GetDescription(_localizer));
                    };
                }
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

        private void LoadInventoryItems(GameObject slot, Guid? typeId = null)
        {
            _componentsContainer.SetActive(true);

            InventoryItemsList.LoadInventoryItems(
                slot,
                _componentsContainer,
                _inventoryRowPrefab,
                _playerState.PlayerInventory,
                HandleRowToggle,
                typeId
            );
        }

        private void HandleRowToggle(GameObject row, GameObject slot, ItemBase item)
        {
            var toggle = row.GetComponent<Toggle>();

            toggle.onValueChanged.AddListener(_ =>
            {
                Tooltips.HideTooltip();

                if (!Enum.TryParse<SlotGameObjectName>(slot.name, out var slotGameObjectName))
                {
                    Debug.LogError($"Failed to find slot for name {slot.name}");
                    return;
                }

                var playerInventory = (PlayerInventory)_playerState.Inventory;
                playerInventory.EquipItemServerRpc(item.Id, slotGameObjectName);
            });
        }

    }
}
