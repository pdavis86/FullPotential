using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Tooltips;
using FullPotential.Core.Player;
using FullPotential.Core.Ui.Components;
using FullPotential.Core.UI.Behaviours;

using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

using FullPotential.Api.CoreTypeIds;
using System;

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

        private IAuditor _logger;
        private ILocalizer _localizer;
        private ITypeRegistry _typeRegistry;

        private GameObject _lastClickedSlot;
        private PlayerFighter _playerFighter;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _logger = DependenciesContext.Dependencies.GetService<IAuditorFactory>().Create(this);
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
                InstantiateSlot(new SlotId(slotType.TypeId), slotType.SlotSpritePrefabAddress, _lhs.transform);
            }

            var accessoryTypes = _typeRegistry.GetRegisteredTypes<IAccessoryType>();
            foreach (var type in accessoryTypes)
            {
                for (var i = 1; i <= type.SlotCount; i++)
                {
                    InstantiateSlot(SlotStatus.GetSlotId(type.TypeId.ToString(), i), type.SlotSpritePrefabAddress, _rhs.transform);
                }
            }

            foreach (var type in _typeRegistry.GetRegisteredTypes<ISlotType>())
            {
                if (type is Registry.SpecialSlots.LeftHand or Registry.SpecialSlots.RightHand)
                {
                    continue;
                }

                InstantiateSlot(new SlotId(type.TypeId), type.SlotSpritePrefabAddress, _rhs.transform);
            }
        }

        private void InstantiateSlot(SlotId slotId, string spritePrefabAddress, Transform parentTransform)
        {
            var equipmentSlot = Instantiate(_slotPrefab, parentTransform);
            equipmentSlot.name = slotId.ToString();

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

            LoadInventoryItems(clickedObject, SlotId.FromString(clickedObject.name));

            _lastClickedSlot = clickedObject;
        }

        private void SetSlot(GameObject slot, ItemBase item)
        {
            if (!slot.TryGetComponent<InventoryUiSlot>(out var slotUi))
            {
                return;
            }

            slotUi.Image.color = item != null ? Color.grey : Color.clear;

            if (slot.TryGetComponent<Tooltip>(out var tooltip))
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
                foreach (var container in new[] { _lhs.transform, _rhs.transform })
                {
                    foreach (Transform slot in container)
                    {
                        if (SlotId.TryParse(slot.name, out var slotId))
                        {
                            var item = _playerFighter.Inventory.GetItemInSlot(slotId);
                            SetSlot(slot.gameObject, item);
                        }
                    }
                }
            }
        }

        private void LoadInventoryItems(GameObject slot, SlotId slotId)
        {
            _componentsContainer.SetActive(true);

            InventoryItemsList.LoadInventoryItems(
                _logger,
                slot,
                _componentsContainer,
                _inventoryRowPrefab,
                _playerFighter.PlayerInventory,
                HandleRowToggle,
                slotId
            );
        }

        private void HandleRowToggle(GameObject row, GameObject slot, ItemBase item)
        {
            var toggle = row.GetComponent<Toggle>();

            toggle.onValueChanged.AddListener(_ =>
            {
                Tooltips.HideTooltip();
                EquipItem(item, slot.name);
                ResetEquipmentUi(true);
            });
        }

        private void EquipItem(ItemBase item, string slotIdString)
        {
            var playerInventory = (PlayerInventory)_playerFighter.Inventory;
            playerInventory.EquipItemServerRpc(item.Id, slotIdString);
        }
    }
}
