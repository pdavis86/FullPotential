using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Ui;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Tooltips;
using FullPotential.Core.Player;
using FullPotential.Core.Ui.Components;
using FullPotential.Core.UI.Behaviours;
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
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private GameObject _inventoryRowPrefab;
#pragma warning restore 0649

        private ILocalizer _localizer;
        private ITypeRegistry _typeRegistry;

        private GameObject _lastClickedSlot;
        private PlayerFighter _playerFighter;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

            InstantiateSlots();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            _playerFighter = GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerFighter>();
            ResetEquipmentUi(true);
        }

        private void InstantiateSlots()
        {
            var armorTypes = _typeRegistry.GetRegisteredTypes<IArmorType>();

            foreach (var slotType in armorTypes)
            {
                InstantiateSlot(slotType.TypeId.ToString(), slotType.SlotSpritePrefabAddress, _lhs.transform);
            }

            var accessoryTypes = _typeRegistry.GetRegisteredTypes<IAccessoryType>();
            foreach (var type in accessoryTypes)
            {
                for (var i = 1; i <= type.SlotCount; i++)
                {
                    InstantiateSlot(Accessory.GetSlotId(type.TypeId.ToString(), i), type.SlotSpritePrefabAddress, _rhs.transform);
                }
            }

            foreach (var type in _typeRegistry.GetRegisteredTypes<IRegisterableWithSlotType>())
            {
                if (type is Registry.SpecialSlots.LeftHand or Registry.SpecialSlots.RightHand)
                {
                    continue;
                }

                InstantiateSlot(type.TypeId.ToString(), type.SlotSpritePrefabAddress, _rhs.transform);
            }
        }

        private void InstantiateSlot(string slotId, string spritePrefabAddress, Transform parentTransform)
        {
            var equipmentSlot = Instantiate(_slotPrefab, parentTransform);
            equipmentSlot.name = slotId;

            var button = equipmentSlot.GetComponent<Button>();
            button.onClick.AddListener(() => OnSlotClick(equipmentSlot));

            _typeRegistry.LoadAddessable<Sprite>(
                spritePrefabAddress,
                sprite =>
                {
                    var image = equipmentSlot.FindInDescendants("PlaceholderImage").GetComponent<Image>();
                    image.sprite = sprite;
                });
        }

        // ReSharper disable once UnusedMember.Global
        private void OnSlotClick(GameObject clickedObject)
        {
            if (_lastClickedSlot == clickedObject)
            {
                ResetEquipmentUi();
                return;
            }

            LoadInventoryItems(clickedObject, clickedObject.name);

            _lastClickedSlot = clickedObject;
        }

        private void SetSlot(GameObject slot, ItemBase item)
        {
            var slotUi = slot.GetComponent<InventoryUiSlot>();

            if (slotUi == null)
            {
                return;
            }

            slotUi.Image.color = item != null ? Color.grey : Color.clear;

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
                else
                {
                    var registerable = _typeRegistry.GetAnyRegisteredBySlotId(slot.name);
                    var translation = _localizer.Translate(registerable);

                    tooltip.OnPointerEnterForTooltip += _ =>
                    {
                        Tooltips.ShowTooltip(translation);
                    };
                }
            }
        }

        public void ResetEquipmentUi(bool reloadSlots = false)
        {
            _componentsContainer.SetActive(false);
            _componentsContainer.transform.DestroyChildren();
            _lastClickedSlot = null;

            if (reloadSlots)
            {
                foreach (Transform slot in _lhs.transform)
                {
                    var item = _playerFighter.Inventory.GetItemInSlot(slot.name);
                    SetSlot(slot.gameObject, item);
                }

                foreach (Transform slot in _rhs.transform)
                {
                    var item = _playerFighter.Inventory.GetItemInSlot(slot.name);
                    SetSlot(slot.gameObject, item);

                    if (item is Weapon weapon && weapon.IsTwoHanded)
                    {
                        var otherSlotName = slot.name == HandSlotIds.LeftHand
                            ? HandSlotIds.RightHand
                            : HandSlotIds.LeftHand;
                        SetSlot(_rhs.transform.Find(otherSlotName).gameObject, null);
                    }
                }
            }
        }

        private void LoadInventoryItems(GameObject slot, string typeId)
        {
            _componentsContainer.SetActive(true);

            InventoryItemsList.LoadInventoryItems(
                slot,
                _componentsContainer,
                _inventoryRowPrefab,
                _playerFighter.PlayerInventory,
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

                var playerInventory = (PlayerInventory)_playerFighter.Inventory;
                playerInventory.EquipItemServerRpc(item.Id, slot.name);
            });
        }

    }
}
