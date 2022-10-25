using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.PlayerBehaviours;
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

        private PlayerState _playerState;
        private PlayerBehaviour _playerBehaviour;
        private IResultFactory _resultFactory;
        private List<ItemBase> _components;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _components = new List<ItemBase>();

            _playerState = GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerState>();
            _playerBehaviour = _playerState.gameObject.GetComponent<PlayerBehaviour>();

            _resultFactory = GameManager.Instance.GetService<IResultFactory>();

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
            var selectedType = _craftingSelector.GetCraftingCategory().Key.Name;
            var selectedSubType = _craftingSelector.GetCraftableTypeName(selectedType);
            var isTwoHanded = _craftingSelector.IsTwoHandedSelected();

            _playerBehaviour.CraftItemServerRpc(componentIds, selectedType, selectedSubType, isTwoHanded, _craftName.text);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void AddComponent(string itemId)
        {
            var item = _playerState.Inventory.GetItemWithId<ItemBase>(itemId);

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
                _playerState.Inventory,
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

            var craftingCategory = _craftingSelector.GetCraftingCategory().Key.Name;

            var craftedItem = _resultFactory.GetCraftedItem(
                craftingCategory,
                _craftingSelector.GetCraftableTypeName(craftingCategory),
                _craftingSelector.IsTwoHandedSelected(),
                _components
            );

            var errors = _playerState.Inventory.ValidateIsCraftable(_components.Select(x => x.Id).ToArray(), craftedItem);
            if (errors.Any())
            {
                _craftErrors.text = string.Join(System.Environment.NewLine, errors);
                _craftButton.interactable = false;
                return;
            }

            _craftButton.interactable = true;
            _outputText.text = _resultFactory.GetItemDescription(craftedItem, false);
        }

    }
}
