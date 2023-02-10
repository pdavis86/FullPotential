using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.Player;
using FullPotential.Core.UI.Components;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.UI.Behaviours
{
    public class AdminShopUi : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private CraftingSelector _craftingSelector;
        [SerializeField] private GameObject _nameAndTogglePrefab;
        [SerializeField] private GameObject _nameAndValueSliderPrefab;
        [SerializeField] private GameObject _attributesScrollViewContentParent;
        [SerializeField] private GameObject _effectsScrollViewContentParent;
        [SerializeField] private GameObject _targetingScrollViewContentParent;
        [SerializeField] private GameObject _shapesScrollViewContentParent;
        [SerializeField] private Text _resultsText;
        [SerializeField] private Text _itemNameText;
#pragma warning restore 0649

        private IResultFactory _resultFactory;
        private ILocalizer _localizer;

        private List<IEffect> _registeredEffects;
        private List<ITargeting> _registeredTargetingOptions;
        private List<IShape> _registeredShapes;

        private List<NameAndValueSlider> _attributeSliderBehaviours;
        private List<NameAndToggle> _attributeToggleBehaviours;
        private List<NameAndToggle> _effectToggleBehaviours;
        private List<NameAndToggle> _targetingToggleBehaviours;
        private List<NameAndToggle> _shapeToggleBehaviours;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _resultFactory = DependenciesContext.Dependencies.GetService<IResultFactory>();
            _localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();

            var typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _registeredEffects = typeRegistry.GetRegisteredTypes<IEffect>().ToList();
            _registeredTargetingOptions = typeRegistry.GetRegisteredTypes<ITargeting>().ToList();
            _registeredShapes = typeRegistry.GetRegisteredTypes<IShape>().ToList();

            InstantiateAttributeControls();
            InstantiateEffectControls();
            InstantiateTargetingControls();
            InstantiateShapeControls();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            var playerState = GameManager.Instance.GetLocalPlayerGameObject().GetComponent<PlayerState>();

            if (playerState.Inventory.IsInventoryFull())
            {
                playerState.AlertInventoryIsFull();
                GameManager.Instance.UserInterface.HideAllMenus();
            }

            ResetUi();
        }

        private void ResetUi()
        {
            ResetSliders();
            ResetToggles();
            _resultsText.text = null;
            _itemNameText.text = null;
        }

        private void InstantiateAttributeControls()
        {
            _attributeSliderBehaviours = new List<NameAndValueSlider>();
            _attributeToggleBehaviours = new List<NameAndToggle>();

            foreach (var field in typeof(Attributes).GetFields())
            {
                if (field.FieldType == typeof(int))
                {
                    var slider = Instantiate(_nameAndValueSliderPrefab, _attributesScrollViewContentParent.transform);
                    var behaviour = slider.GetComponent<NameAndValueSlider>();
                    behaviour.Name.text = field.Name;

                    _attributeSliderBehaviours.Add(behaviour);
                }
                else if (field.FieldType == typeof(bool))
                {
                    var toggle = Instantiate(_nameAndTogglePrefab, _attributesScrollViewContentParent.transform);
                    var behaviour = toggle.GetComponent<NameAndToggle>();
                    behaviour.Name.text = field.Name;

                    _attributeToggleBehaviours.Add(behaviour);
                }
                else if (field.Name == nameof(Attributes.ExtraAmmoPerShot))
                {
                    var slider = Instantiate(_nameAndValueSliderPrefab, _attributesScrollViewContentParent.transform);
                    var behaviour = slider.GetComponent<NameAndValueSlider>();
                    behaviour.Name.text = field.Name;
                    behaviour.Slider.maxValue = ResultFactory.MaxExtraAmmo;

                    _attributeSliderBehaviours.Add(behaviour);

                    //Work-around for UI quirk
                    behaviour.Slider.value = 1;
                    behaviour.Slider.value = 0;
                }
                else
                {
                    Debug.LogWarning("Unhandled Attributes type: " + field.FieldType);
                }
            }

            ResetSliders();
        }

        private void InstantiateEffectControls()
        {
            _effectToggleBehaviours = new List<NameAndToggle>();

            foreach (var effect in _registeredEffects)
            {
                var toggle = Instantiate(_nameAndTogglePrefab, _effectsScrollViewContentParent.transform);
                var behaviour = toggle.GetComponent<NameAndToggle>();
                behaviour.Name.text = effect.TypeName;

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
                behaviour.Name.text = option.TypeName;

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
                behaviour.Name.text = shape.TypeName;

                _shapeToggleBehaviours.Add(behaviour);
            }
        }

        private void ResetSliders()
        {
            foreach (var behaviour in _attributeSliderBehaviours)
            {
                behaviour.Slider.value = Mathf.Approximately(behaviour.Slider.maxValue, 100)
                    ? ValueCalculator.Random.Next(1, 101)
                    : 0;
            }
        }

        private void ResetToggles()
        {
            var allToggles = _attributeToggleBehaviours
                .Union(_effectToggleBehaviours)
                .Union(_targetingToggleBehaviours)
                .Union(_shapeToggleBehaviours);

            foreach (var behaviour in allToggles)
            {
                behaviour.Toggle.isOn = false;
            }
        }

        private Attributes GetAttributes()
        {
            var attributes = (object)new Attributes();

            foreach (var field in typeof(Attributes).GetFields())
            {
                if (field.FieldType == typeof(int))
                {
                    var slider = _attributeSliderBehaviours.First(s => s.Name.text == field.Name);
                    field.SetValue(attributes, (int)slider.Slider.value);
                }
                else if (field.FieldType == typeof(bool))
                {
                    var toggle = _attributeToggleBehaviours.First(t => t.Name.text == field.Name);
                    field.SetValue(attributes, toggle.Toggle.isOn);
                }
                else if (field.FieldType == typeof(byte))
                {
                    var slider = _attributeSliderBehaviours.First(s => s.Name.text == field.Name);
                    field.SetValue(attributes, (byte)slider.Slider.value);
                }
            }

            return (Attributes)attributes;
        }

        private List<IEffect> GetEffects()
        {
            var effects = new List<IEffect>();

            foreach (var effect in _registeredEffects)
            {
                var toggle = _effectToggleBehaviours.First(t => t.Name.text == effect.TypeName);
                if (toggle.Toggle.isOn)
                {
                    effects.Add(effect);
                }
            }

            return effects;
        }

        private ITargeting GetTargeting()
        {
            var firstToggle = _targetingToggleBehaviours.FirstOrDefault(x => x.Toggle.isOn);
            var typeName = firstToggle != null ? firstToggle.Name.text : null;
            return _registeredTargetingOptions.FirstOrDefault(x => x.TypeName == typeName);
        }

        private IShape GetShape()
        {
            var firstToggle = _shapeToggleBehaviours.FirstOrDefault(x => x.Toggle.isOn);
            var typeName = firstToggle != null ? firstToggle.Name.text : null;
            return _registeredShapes.FirstOrDefault(x => x.TypeName == typeName);
        }

        private Loot GetLootFromChoices()
        {
            return new Loot
            {
                Attributes = GetAttributes(),
                Effects = GetEffects(),
                Targeting = GetTargeting(),
                Shape = GetShape()
            };
        }

        private ItemBase GetCraftableItem()
        {
            var category = _craftingSelector.GetCraftingCategory().Key.Name;
            var component = GetLootFromChoices();

            return _resultFactory.GetCraftedItem(
                category,
                _craftingSelector.GetCraftableTypeName(category),
                _craftingSelector.IsTwoHandedSelected(),
                new List<ItemBase> { component });
        }

        // ReSharper disable once UnusedMember.Global
        public void DisplayResultsText()
        {
            var item = GetCraftableItem();
            _resultsText.text = item.GetDescription(_localizer, LevelOfDetail.Full, _itemNameText.text);
        }

        // ReSharper disable once UnusedMember.Global
        public void CraftItem()
        {
            var component = GetLootFromChoices();
            var componentJson = JsonUtility.ToJson(component);
            var category = _craftingSelector.GetCraftingCategory().Key.Name;

            GameManager.Instance.GetLocalPlayerGameObject().GetComponent<PlayerBehaviour>().CraftItemAsAdminServerRpc(
                componentJson,
                category,
                _craftingSelector.GetCraftableTypeName(category),
                _craftingSelector.IsTwoHandedSelected(),
                _itemNameText.text
                );

            ResetUi();
        }
    }
}
