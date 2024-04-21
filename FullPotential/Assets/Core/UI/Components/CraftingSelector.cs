using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.UI.Components
{
    public class CraftingSelector : MonoBehaviour
    {
        // ReSharper disable UnassignedField.Global
        public Dropdown TypeDropdown;
        public Dropdown SubTypeDropdown;
        public Dropdown HandednessDropdown;
        public Dropdown ResourceDropdown;
        // ReSharper restore UnassignedField.Global

        private Dictionary<CraftableType, string> _craftableTypes;
        private Dictionary<IAccessoryType, string> _accessoryTypes;
        private Dictionary<IArmorType, string> _armorTypes;
        private Dictionary<IResourceType, string> _resourceTypes;
        private Dictionary<IWeaponType, string> _weaponTypes;
        private Dictionary<ISpecialGearType, string> _specialTypes;
        private List<string> _handednessOptions;
        private List<int?> _optionalTwoHandedWeaponIndexes;

        #region "Event Handlers"

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            FillDataVariables();

            TypeDropdown.ClearOptions();
            TypeDropdown.AddOptions(_craftableTypes.Select(x => x.Value).ToList());

            TypeDropdown.onValueChanged.AddListener(TypeOnValueChanged);

            HandednessDropdown.ClearOptions();
            HandednessDropdown.AddOptions(_handednessOptions);

            UpdateSecondaryDropDowns();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            TypeDropdown.value = 0;
        }

        private void TypeOnValueChanged(int index)
        {
            UpdateSecondaryDropDowns();
        }

        #endregion

        public CraftableType GetTypeToCraft()
        {
            return _craftableTypes.ElementAt(TypeDropdown.value).Key;
        }

        public string GetSubTypeId(CraftableType craftableType)
        {
            switch (craftableType)
            {
                case CraftableType.Accessory: return _accessoryTypes.ElementAt(SubTypeDropdown.value).Key.TypeId.ToString();
                case CraftableType.Armor: return _armorTypes.ElementAt(SubTypeDropdown.value).Key.TypeId.ToString();
                case CraftableType.Weapon: return _weaponTypes.ElementAt(SubTypeDropdown.value).Key.TypeId.ToString();
                case CraftableType.Consumer: return null;
                case CraftableType.SpecialGear: return _specialTypes.ElementAt(SubTypeDropdown.value).Key.TypeId.ToString();
                default: throw new InvalidOperationException($"Unknown craftable type: '{craftableType}'");
            }
        }

        public string GetResourceTypeId()
        {
            return _resourceTypes.ElementAt(ResourceDropdown.value).Key.TypeId.ToString();
        }

        public bool IsTwoHandedSelected()
        {
            return GetTypeToCraft() == CraftableType.Weapon
                && HandednessDropdown.options.Count > 0
                && HandednessDropdown.value == 1;
        }

        private void UpdateSecondaryDropDowns()
        {
            SubTypeDropdown.ClearOptions();
            ResourceDropdown.gameObject.SetActive(false);

            var typeToCraft = GetTypeToCraft();
            switch (typeToCraft)
            {
                case CraftableType.Accessory:
                    SubTypeDropdown.AddOptions(_accessoryTypes.Select(x => x.Value).ToList());
                    break;

                case CraftableType.Armor:
                    SubTypeDropdown.AddOptions(_armorTypes.Select(x => x.Value).ToList());
                    break;

                case CraftableType.Weapon:
                    SubTypeDropdown.AddOptions(_weaponTypes.Select(x => x.Value).ToList());
                    break;

                case CraftableType.Consumer:
                    SetupResourcesDropDown();
                    break;

                case CraftableType.SpecialGear:
                    SubTypeDropdown.AddOptions(_specialTypes.Select(x => x.Value).ToList());
                    SetupResourcesDropDown();
                    break;

                default: throw new InvalidOperationException($"Unknown crafting type: '{typeToCraft}'");
            }

            if (SubTypeDropdown.options.Any())
            {
                SubTypeDropdown.RefreshShownValue();
                SubTypeDropdown.gameObject.SetActive(true);
            }
            else
            {
                SubTypeDropdown.gameObject.SetActive(false);
            }

            HandednessDropdown.gameObject.SetActive(typeToCraft == CraftableType.Weapon && _optionalTwoHandedWeaponIndexes.Contains(SubTypeDropdown.value));
        }

        private void SetupResourcesDropDown()
        {
            var options = _resourceTypes.Select(kvp => kvp.Value).ToList();

            ResourceDropdown.ClearOptions();
            ResourceDropdown.AddOptions(options);
            ResourceDropdown.gameObject.SetActive(true);
        }

        private void FillDataVariables()
        {
            var localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            var typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

            _craftableTypes = localizer.GetDictionaryFromEnum<CraftableType>(TranslationType.ItemType);

            _accessoryTypes = typeRegistry.GetRegisteredTypes<IAccessoryType>()
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _armorTypes = typeRegistry.GetRegisteredTypes<IArmorType>()
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _resourceTypes = typeRegistry.GetRegisteredTypes<IResourceType>()
                .Where(r => r.IsCraftable)
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _weaponTypes = typeRegistry.GetRegisteredTypes<IWeaponType>()
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _specialTypes = typeRegistry.GetRegisteredTypes<ISpecialGearType>()
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _handednessOptions = new List<string> {
                localizer.Translate(TranslationType.CraftingHandedness, "one"),
                localizer.Translate(TranslationType.CraftingHandedness, "two")
            };

            _optionalTwoHandedWeaponIndexes = _weaponTypes
                .Select((x, i) => !x.Key.EnforceTwoHanded && x.Key.AllowTwoHanded ? (int?)i : null)
                .Where(x => x != null)
                .OrderBy(x => x)
                .ToList();
        }
    }
}
