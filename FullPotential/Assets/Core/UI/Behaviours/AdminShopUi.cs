using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Player;
using FullPotential.Core.UI.Components;

using UnityEngine;
using UnityEngine.UI;


namespace FullPotential.Core.UI.Behaviours
{
    public class AdminShopUi : MonoBehaviour
    {
        private readonly System.Random _random = new System.Random();

#pragma warning disable 0649
        [SerializeField] private CraftingSelector _craftingSelector;
        [SerializeField] private GameObject _nameAndTogglePrefab;
        [SerializeField] private GameObject _nameAndValueSliderPrefab;
        [SerializeField] private GameObject _attributesScrollViewContentParent;
        [SerializeField] private GameObject _effectsScrollViewContentParent;
        [SerializeField] private GameObject _targetingScrollViewContentParent;
        [SerializeField] private GameObject _shapesScrollViewContentParent;
        [SerializeField] private Text _resultsText;
        [SerializeField] private InputField _itemNameInputField;
#pragma warning restore 0649

        private IResultFactory _resultFactory;
        private ILocalizer _localizer;
        private IItemFactory _itemFactory;
        private ITypeRegistry _typeRegistry;

        private List<IEffectType> _registeredEffects;
        private List<ITargetingType> _registeredTargetingOptions;
        private List<IShapeType> _registeredShapes;

        private List<NameAndValueSlider> _attributeSliderBehaviours;
        private List<NameAndValueSlider> _propertySliderBehaviours;
        private List<NameAndToggle> _propertyToggleBehaviours;
        private List<NameAndToggle> _effectToggleBehaviours;
        private List<NameAndToggle> _targetingToggleBehaviours;
        private List<NameAndToggle> _shapeToggleBehaviours;

        private void Awake()
        {
            _resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            _itemFactory = DependenciesContext.Dependencies.GetService<IItemFactory>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

            _registeredEffects = _typeRegistry.GetRegisteredTypes<IEffectType>().ToList();
            _registeredTargetingOptions = _typeRegistry.GetRegisteredTypes<ITargetingType>().ToList();
            _registeredShapes = _typeRegistry.GetRegisteredTypes<IShapeType>().ToList();

            InstantiateAttributeControls();
            InstantiateEffectControls();
            InstantiateTargetingControls();
            InstantiateShapeControls();
        }

        private void OnEnable()
        {
            // todo: zzz v0.7 - Handle when inventory is full
            //var playerState = GameManager.Instance.GetLocalPlayerGameObject().GetComponent<PlayerFighter>();
            //if (playerState.PlayerInventory.IsInventoryFull())
            //{
            //    playerState.AlertInventoryIsFull();
            //    GameManager.Instance.UserInterface.HideAllMenus();
            //}

            ResetUi();
        }

        private void ResetUi()
        {
            ResetSliders();
            ResetToggles();
            _resultsText.text = null;
            _itemNameInputField.text = null;
        }

        private void InstantiateAttributeControls()
        {
            _attributeSliderBehaviours = new List<NameAndValueSlider>();
            _propertySliderBehaviours = new List<NameAndValueSlider>();
            _propertyToggleBehaviours = new List<NameAndToggle>();

            foreach (var attribute in _typeRegistry.GetRegisteredTypes<IAttribute>())
            {
                var slider = Instantiate(_nameAndValueSliderPrefab, _attributesScrollViewContentParent.transform);
                var behaviour = slider.GetComponent<NameAndValueSlider>();
                behaviour.Id = attribute.TypeId.ToString();
                behaviour.Name.text = attribute.GetType().Name;
                _attributeSliderBehaviours.Add(behaviour);
            }

            var extraAmmoSlider = Instantiate(_nameAndValueSliderPrefab, _attributesScrollViewContentParent.transform);
            var extraAmmoBehaviour = extraAmmoSlider.GetComponent<NameAndValueSlider>();
            extraAmmoBehaviour.Id = "ExtraAmmoPerShot";
            extraAmmoBehaviour.Name.text = "ExtraAmmoPerShot";
            extraAmmoBehaviour.Slider.maxValue = ResultFactory.MaxExtraAmmo;
            extraAmmoBehaviour.Slider.value = 1;
            extraAmmoBehaviour.Slider.value = 0;
            _propertySliderBehaviours.Add(extraAmmoBehaviour);

            var automaticToggle = Instantiate(_nameAndTogglePrefab, _attributesScrollViewContentParent.transform);
            var automaticBehaviour = automaticToggle.GetComponent<NameAndToggle>();
            automaticBehaviour.Id = "IsAutomatic";
            automaticBehaviour.Name.text = "IsAutomatic";
            _propertyToggleBehaviours.Add(automaticBehaviour);

            ResetSliders();
        }

        private void InstantiateEffectControls()
        {
            _effectToggleBehaviours = new List<NameAndToggle>();

            foreach (var effect in _registeredEffects)
            {
                var toggle = Instantiate(_nameAndTogglePrefab, _effectsScrollViewContentParent.transform);
                var behaviour = toggle.GetComponent<NameAndToggle>();
                behaviour.Name.text = effect.GetType().Name;

                _effectToggleBehaviours.Add(behaviour);
            }
        }

        private void InstantiateTargetingControls()
        {
            _targetingToggleBehaviours = new List<NameAndToggle>();

            foreach (var option in _registeredTargetingOptions)
            {
                var toggle = Instantiate(_nameAndTogglePrefab, _targetingScrollViewContentParent.transform);
                var behaviour = toggle.GetComponent<NameAndToggle>();
                behaviour.Name.text = option.GetType().Name;

                _targetingToggleBehaviours.Add(behaviour);
            }
        }

        private void InstantiateShapeControls()
        {
            _shapeToggleBehaviours = new List<NameAndToggle>();

            foreach (var shape in _registeredShapes)
            {
                var toggle = Instantiate(_nameAndTogglePrefab, _shapesScrollViewContentParent.transform);
                var behaviour = toggle.GetComponent<NameAndToggle>();
                behaviour.Name.text = shape.GetType().Name;

                _shapeToggleBehaviours.Add(behaviour);
            }
        }

        private void ResetSliders()
        {
            foreach (var collection in new[] { _attributeSliderBehaviours, _propertySliderBehaviours })
            {
                foreach (var behaviour in collection)
                {
                    behaviour.Slider.value = Mathf.Approximately(behaviour.Slider.maxValue, 100)
                        ? _random.Next(1, 101)
                        : 0;
                }
            }
        }

        private void ResetToggles()
        {
            var allToggles = _propertyToggleBehaviours
                .Union(_effectToggleBehaviours)
                .Union(_targetingToggleBehaviours)
                .Union(_shapeToggleBehaviours);

            foreach (var behaviour in allToggles)
            {
                behaviour.Toggle.isOn = false;
            }
        }

        private Dictionary<AttributeTypeId, int> GetAttributes()
        {
            var attributes = new Dictionary<AttributeTypeId, int>();

            foreach (var slider in _attributeSliderBehaviours)
            {
                attributes[AttributeTypeId.FromString(slider.Id)] = (int)slider.Slider.value;
            }

            return attributes;
        }

        private Dictionary<string, string> GetProperties()
        {
            var properties = new Dictionary<string, string>();

            foreach (var slider in _propertySliderBehaviours)
            {
                properties[slider.Id] = slider.Slider.value.ToString();
            }

            foreach (var toggle in _propertyToggleBehaviours)
            {
                properties[toggle.Id] = toggle.Toggle.isOn.ToString();
            }

            return properties;
        }

        private List<IEffectType> GetEffects()
        {
            var effects = new List<IEffectType>();

            foreach (var effect in _registeredEffects)
            {
                var toggle = _effectToggleBehaviours.First(t => t.Name.text == effect.GetType().Name);
                if (toggle.Toggle.isOn)
                {
                    effects.Add(effect);
                }
            }

            return effects;
        }

        private ITargetingType GetTargeting()
        {
            var firstToggle = _targetingToggleBehaviours.FirstOrDefault(x => x.Toggle.isOn);
            var typeName = firstToggle != null ? firstToggle.Name.text : null;
            return _registeredTargetingOptions.FirstOrDefault(x => x.GetType().Name == typeName);
        }

        private IShapeType GetShape()
        {
            var firstToggle = _shapeToggleBehaviours.FirstOrDefault(x => x.Toggle.isOn);
            var typeName = firstToggle != null ? firstToggle.Name.text : null;
            return _registeredShapes.FirstOrDefault(x => x.GetType().Name == typeName);
        }

        private CombatItemBase GetLootFromChoices()
        {
            var lootItem = DependenciesContext.Dependencies.CreateInstance<AdminCraftingLootItem>();
            lootItem.RegistryType = _typeRegistry.GetRegisteredTypes<ILootType>().First();
            lootItem.Attributes = GetAttributes();
            lootItem.Properties = GetProperties();
            lootItem.EffectTypes = GetEffects();
            lootItem.TargetingType = GetTargeting();
            lootItem.ShapeType = GetShape();
            return lootItem;
        }

        private ItemBase GetCraftableItem()
        {
            var typeToCraft = _craftingSelector.GetTypeToCraft();
            var component = GetLootFromChoices();

            return _resultFactory.GetCraftedItem(
                typeToCraft,
                _craftingSelector.GetSubTypeId(typeToCraft),
                _craftingSelector.GetValuePoolTypeId(),
                _craftingSelector.IsTwoHandedSelected(),
                _itemNameInputField.text,
                new List<ItemBase> { component });
        }

        public void DisplayResultsText()
        {
            var item = GetCraftableItem();
            _resultsText.text = item.GetDescription(_localizer, LevelOfDetail.Full);
        }

        public void CraftItem()
        {
            var component = GetLootFromChoices();

            var playerFighter = GameManager.Instance.GetLocalPlayerGameObject().GetComponent<PlayerFighter>();
            var itemData = _itemFactory.GetDataFromItem(playerFighter.CharacterId, component);

            var craftableType = _craftingSelector.GetTypeToCraft();

            GameManager.Instance.GetLocalPlayerGameObject().GetComponent<PlayerBehaviour>().CraftItemAsAdminServerRpc(
                itemData.ToJson(),
                craftableType.Name,
                _craftingSelector.GetSubTypeId(craftableType),
                _craftingSelector.GetValuePoolTypeId(),
                _craftingSelector.IsTwoHandedSelected(),
                _itemNameInputField.text);

            ResetUi();
        }
    }
}
