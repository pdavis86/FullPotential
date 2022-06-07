using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using FullPotential.Core.Gameplay.Combat;
using FullPotential.Core.Gameplay.Crafting;
using FullPotential.Core.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace FullPotential.Standard.Scenes.Behaviours
{
    public class ShopUi : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] private CraftingSelector _craftingSelector;
        [SerializeField] private GameObject _nameAndTogglePrefab;
        [SerializeField] private GameObject _nameAndValueSliderPrefab;
        [SerializeField] private GameObject _attributesScrollViewContentParent;
        [SerializeField] private GameObject _effectsScrollViewContentParent;
        [SerializeField] private GameObject _targetingScrollViewContentParent;
        [SerializeField] private GameObject _shapesScrollViewContentParent;
        [SerializeField] private Text _resultsText;
        [SerializeField] private Text _itemNameText;
#pragma warning restore CS0649

        private ResultFactory _resultFactory;
        private List<IEffect> _registeredEffects;
        private List<ITargeting> _registeredTargetingOptions;
        private List<IShape> _registeredShapes;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            var gameManager = ModHelper.GetGameManager();

            _resultFactory = gameManager.GetService<ResultFactory>();

            var typeRegistry = gameManager.GetService<ITypeRegistry>();
            _registeredEffects = typeRegistry.GetRegisteredTypes<IEffect>().ToList();
            _registeredTargetingOptions = typeRegistry.GetRegisteredTypes<ITargeting>().ToList();
            _registeredShapes = typeRegistry.GetRegisteredTypes<IShape>().ToList();

            InstantiateAttributeControls();
            InstantiateEffectControls();
            InstantiateTargetingControls();
            InstantiateShapeControls();
        }

        private void ResetUi()
        {
            _resultsText.text = null;
        }

        private void InstantiateAttributeControls()
        {
            foreach (var field in typeof(Attributes).GetFields())
            {
                if (field.FieldType == typeof(int))
                {
                    var slider = Instantiate(_nameAndValueSliderPrefab, _attributesScrollViewContentParent.transform);
                    var behaviour = slider.GetComponent<NameAndValueSlider>();
                    behaviour.Name.text = field.Name;
                    behaviour.Slider.value = AttributeCalculator.Random.Next(1, 101);
                }
                else if (field.FieldType == typeof(bool))
                {
                    var toggle = Instantiate(_nameAndTogglePrefab, _attributesScrollViewContentParent.transform);
                    var behaviour = toggle.GetComponent<NameAndToggle>();
                    behaviour.Name.text = field.Name;
                    behaviour.Toggle.isOn = false;
                }
                else if (field.Name == nameof(Attributes.ExtraAmmoPerShot))
                {
                    var slider = Instantiate(_nameAndValueSliderPrefab, _attributesScrollViewContentParent.transform);
                    var behaviour = slider.GetComponent<NameAndValueSlider>();
                    behaviour.Name.text = field.Name;
                    behaviour.Slider.maxValue = ResultFactory.MaxExtraAmmo;
                    behaviour.Slider.value = 1;
                    behaviour.Slider.value = 0;
                }
                else
                {
                    Debug.LogWarning("Unhandled Attributes type: " + field.FieldType);
                }
            }
        }

        private void InstantiateEffectControls()
        {
            foreach (var effect in _registeredEffects)
            {
                var toggle = Instantiate(_nameAndTogglePrefab, _effectsScrollViewContentParent.transform);
                var behaviour = toggle.GetComponent<NameAndToggle>();
                behaviour.Name.text = effect.TypeName;
                behaviour.Toggle.isOn = false;
            }
        }

        private void InstantiateTargetingControls()
        {
            foreach (var option in _registeredTargetingOptions)
            {
                var toggle = Instantiate(_nameAndTogglePrefab, _targetingScrollViewContentParent.transform);
                var behaviour = toggle.GetComponent<NameAndToggle>();
                behaviour.Name.text = option.TypeName;
                behaviour.Toggle.isOn = false;
            }
        }

        private void InstantiateShapeControls()
        {
            foreach (var shape in _registeredShapes)
            {
                var toggle = Instantiate(_nameAndTogglePrefab, _shapesScrollViewContentParent.transform);
                var behaviour = toggle.GetComponent<NameAndToggle>();
                behaviour.Name.text = shape.TypeName;
                behaviour.Toggle.isOn = false;
            }
        }

        private Attributes GetAttributes()
        {
            var sliders = _attributesScrollViewContentParent.transform
                .GetComponentsInChildren(typeof(NameAndValueSlider))
                .Select(c => c.GetComponent<NameAndValueSlider>());

            var toggles = _attributesScrollViewContentParent.transform
                .GetComponentsInChildren(typeof(NameAndToggle))
                .Select(c => c.GetComponent<NameAndToggle>());

            var attributes = (object)new Attributes();

            foreach (var field in typeof(Attributes).GetFields())
            {
                if (field.FieldType == typeof(int))
                {
                    var slider = sliders.First(s => s.Name.text == field.Name);
                    field.SetValue(attributes, (int)slider.Slider.value);
                }
                else if (field.FieldType == typeof(bool))
                {
                    var toggle = toggles.First(t => t.Name.text == field.Name);
                    field.SetValue(attributes, toggle.Toggle.isOn);
                }
                else if (field.FieldType == typeof(byte))
                {
                    var slider = sliders.First(s => s.Name.text == field.Name);
                    field.SetValue(attributes, (byte)slider.Slider.value);
                }
            }

            return (Attributes)attributes;
        }

        private List<IEffect> GetEffects()
        {
            var toggles = _effectsScrollViewContentParent.transform
                .GetComponentsInChildren(typeof(NameAndToggle))
                .Select(c => c.GetComponent<NameAndToggle>());

            var effects = new List<IEffect>();

            foreach (var effect in _registeredEffects)
            {
                var toggle = toggles.First(t => t.Name.text == effect.TypeName);
                if (toggle.Toggle.isOn)
                {
                    effects.Add(effect);
                }
            }

            return effects;
        }

        private ITargeting GetTargeting()
        {
            var typeName = _targetingScrollViewContentParent.transform
                .GetComponentsInChildren(typeof(NameAndToggle))
                .Select(c => c.GetComponent<NameAndToggle>())
                .FirstOrDefault(x => x.Toggle.isOn)
                ?.Name.text;

            return _registeredTargetingOptions.FirstOrDefault(x => x.TypeName == typeName);
        }

        private IShape GetShape()
        {
            var typeName = _shapesScrollViewContentParent.transform
                .GetComponentsInChildren(typeof(NameAndToggle))
                .Select(c => c.GetComponent<NameAndToggle>())
                .FirstOrDefault(x => x.Toggle.isOn)
                ?.Name.text;

            return _registeredShapes.FirstOrDefault(x => x.TypeName == typeName);
        }

        private Api.Registry.Loot.Loot GetLootFromChoices()
        {
            return new Api.Registry.Loot.Loot
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
                new[] { component });
        }

        // ReSharper disable once UnusedMember.Global
        public void DisplayResultsText()
        {
            var item = GetCraftableItem();
            _resultsText.text = _resultFactory.GetItemDescription(item);
        }

        // ReSharper disable once UnusedMember.Global
        public void CraftItem()
        {
            var component = GetLootFromChoices();
            var componentJson = JsonUtility.ToJson(component);
            var category = _craftingSelector.GetCraftingCategory().Key.Name;

            //todo: do not call Core
            ModHelper.GetGameManager().GetLocalPlayerGameObject().GetComponent<Core.PlayerBehaviours.PlayerActions>().CraftItemAsAdminServerRpc(
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
