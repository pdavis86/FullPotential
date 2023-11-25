using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using FullPotential.Core.Ui.Components;
using FullPotential.Core.UI.Components;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Ui.Behaviours
{
    public class CharacterMenuUiCraftingTab : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject _componentsContainer;
        [SerializeField] private CraftingSelector _craftingSelector;
        [SerializeField] private Text _outputText;
        [SerializeField] private InputField _craftName;
        [SerializeField] private Button _craftButton;
        [SerializeField] private Text _craftErrors;
        [SerializeField] private GameObject _inventoryRowPrefab;
#pragma warning restore 0649

        private readonly List<ItemForCombatBase> _components = new List<ItemForCombatBase>();

        private PlayerState _playerState;
        private PlayerBehaviour _playerBehaviour;

        private IResultFactory _resultFactory;
        private ILocalizer _localizer;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _playerState = GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerState>();
            _playerBehaviour = _playerState.gameObject.GetComponent<PlayerBehaviour>();

            _resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();

            _craftButton.onClick.AddListener(CraftButtonOnClick);

            _craftingSelector.TypeDropdown.onValueChanged.AddListener(TypeOnValueChanged);

            _craftingSelector.SubTypeDropdown.onValueChanged.AddListener(SubTypeOnValueChanged);

            _craftingSelector.HandednessDropdown.onValueChanged.AddListener(HandednessOnValueChanged);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            ResetUi();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            _componentsContainer.transform.DestroyChildren();
        }

        private void TypeOnValueChanged(int index)
        {
            UpdateResults();
        }

        private void SubTypeOnValueChanged(int index)
        {
            UpdateResults();
        }

        private void HandednessOnValueChanged(int index)
        {
            UpdateResults();
        }

        private void CraftButtonOnClick()
        {
            _craftButton.interactable = false;

            var componentIds = string.Join(',', _components.Select(x => x.Id));
            var selectedType = _craftingSelector.GetTypeToCraft();
            var selectedSubType = _craftingSelector.GetSubTypeId(selectedType);
            var resourceTypeId = _craftingSelector.GetResourceTypeId();
            var isTwoHanded = _craftingSelector.IsTwoHandedSelected();

            _playerBehaviour.CraftItemServerRpc(componentIds, selectedType.ToString(), selectedSubType, resourceTypeId, isTwoHanded, _craftName.text);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void AddComponent(string itemId)
        {
            var item = _playerState.Inventory.GetItemWithId<ItemForCombatBase>(itemId);

            if (item == null)
            {
                Debug.LogWarning("No item found with id " + itemId);
                return;
            }

            _components.Add(item);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void RemoveComponent(string itemId)
        {
            var item = _components.FirstOrDefault(x => x.Id == itemId);
            if (item != null)
            {
                _components.Remove(item);
            }
        }

        public void ResetUi()
        {
            LoadInventory();

            ResetUiText();

            _craftButton.interactable = false;
        }

        private void ResetUiText()
        {
            _outputText.text = null;
            _craftName.text = null;
            _craftErrors.text = null;
        }

        private void LoadInventory()
        {
            _components.Clear();

            InventoryItemsList.LoadInventoryItems(
                null,
                _componentsContainer,
                _inventoryRowPrefab,
                _playerState.PlayerInventory,
                HandleRowToggle,
                null,
                false
            );
        }

        private void HandleRowToggle(GameObject row, GameObject slot, ItemBase item)
        {
            var rowImage = row.GetComponent<Image>();
            var toggle = row.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    rowImage.color = Color.green;
                    AddComponent(item.Id);
                }
                else
                {
                    rowImage.color = Color.white;
                    RemoveComponent(item.Id);
                }

                UpdateResults();
            });
        }

        private void UpdateResults()
        {
            ResetUiText();

            if (_components.Count == 0)
            {
                _craftButton.interactable = false;
                return;
            }

            var craftableType = _craftingSelector.GetTypeToCraft();

            var craftedItem = _resultFactory.GetCraftedItem(
                craftableType,
                _craftingSelector.GetSubTypeId(craftableType),
                _craftingSelector.GetResourceTypeId(),
                _craftingSelector.IsTwoHandedSelected(),
                _components
            );

            var errors = _playerState.PlayerInventory.ValidateIsCraftable(_components.Select(x => x.Id).ToArray(), craftedItem);
            if (errors.Any())
            {
                _craftErrors.text = string.Join(System.Environment.NewLine, errors);
                _craftButton.interactable = false;
                return;
            }

            _craftButton.interactable = true;
            _outputText.text = craftedItem.GetDescription(_localizer, LevelOfDetail.Intermediate);
        }

    }
}
