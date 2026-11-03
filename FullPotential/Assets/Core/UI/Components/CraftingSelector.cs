using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;

using UnityEngine;
using UnityEngine.UI;


namespace FullPotential.Core.UI.Components
{
    public class CraftingSelector : MonoBehaviour
    {
        public Dropdown TypeDropdown;
        public Dropdown SubTypeDropdown;
        public Dropdown HandednessDropdown;
        public Dropdown ValuePoolDropdown;

        private Dictionary<Type, string> _craftableTypes;
        private Dictionary<IAccessoryType, string> _accessoryTypes;
        private Dictionary<IArmorType, string> _armorTypes;
        private Dictionary<IValuePoolType, string> _valuePoolTypes;
        private Dictionary<IWeaponType, string> _weaponTypes;
        private Dictionary<ICustomGearType, string> _customGearTypes;
        private List<string> _handednessOptions;
        private List<int?> _optionalTwoHandedWeaponIndexes;

        #region "Event Handlers"

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

        private void OnEnable()
        {
            TypeDropdown.value = 0;
        }

        private void TypeOnValueChanged(int index)
        {
            UpdateSecondaryDropDowns();
        }

        #endregion

        public Type GetTypeToCraft()
        {
            return _craftableTypes.ElementAt(TypeDropdown.value).Key;
        }

        public string GetSubTypeId(Type craftableType)
        {
            if (craftableType == typeof(IAccessoryType))
            {
                return _accessoryTypes.ElementAt(SubTypeDropdown.value).Key.TypeId.ToString();
            }
            else if (craftableType == typeof(IArmorType))
            {
                return _armorTypes.ElementAt(SubTypeDropdown.value).Key.TypeId.ToString();
            }
            else if (craftableType == typeof(IWeaponType))
            {
                return _weaponTypes.ElementAt(SubTypeDropdown.value).Key.TypeId.ToString();
            }
            else if (craftableType == typeof(IConsumerType))
            {
                return null;
            }
            else if (craftableType == typeof(ICustomGearType))
            {
                return _customGearTypes.ElementAt(SubTypeDropdown.value).Key.TypeId.ToString();
            }
            else
            {
                throw new InvalidOperationException($"Unknown craftable type: '{craftableType}'");
            }
        }

        public string GetValuePoolTypeId()
        {
            return _valuePoolTypes.ElementAt(ValuePoolDropdown.value).Key.TypeId.ToString();
        }

        public bool IsTwoHandedSelected()
        {
            return GetTypeToCraft() == typeof(IWeaponType)
                && HandednessDropdown.options.Count > 0
                && HandednessDropdown.value == 1;
        }

        private void UpdateSecondaryDropDowns()
        {
            SubTypeDropdown.ClearOptions();
            ValuePoolDropdown.gameObject.SetActive(false);

            var typeToCraft = GetTypeToCraft();
            if (typeToCraft == typeof(IAccessoryType))
            {
                SubTypeDropdown.AddOptions(_accessoryTypes.Select(x => x.Value).ToList());
            }
            else if (typeToCraft == typeof(IArmorType))
            {
                SubTypeDropdown.AddOptions(_armorTypes.Select(x => x.Value).ToList());
            }
            else if (typeToCraft == typeof(IWeaponType))
            {
                SubTypeDropdown.AddOptions(_weaponTypes.Select(x => x.Value).ToList());
            }
            else if (typeToCraft == typeof(IConsumerType))
            {
                SetupValuePoolsDropDown();
            }
            else if (typeToCraft == typeof(ICustomGearType))
            {
                SubTypeDropdown.AddOptions(_customGearTypes.Select(x => x.Value).ToList());
                SetupValuePoolsDropDown();
            }
            else
            {
                throw new InvalidOperationException($"Unknown crafting type: '{typeToCraft}'");
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

            HandednessDropdown.gameObject.SetActive(typeToCraft == typeof(IWeaponType) && _optionalTwoHandedWeaponIndexes.Contains(SubTypeDropdown.value));
        }

        private void SetupValuePoolsDropDown()
        {
            var options = _valuePoolTypes.Select(kvp => kvp.Value).ToList();

            ValuePoolDropdown.ClearOptions();
            ValuePoolDropdown.AddOptions(options);
            ValuePoolDropdown.gameObject.SetActive(true);
        }

        private void FillDataVariables()
        {
            var localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            var typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

            _craftableTypes = new Dictionary<Type, string>
            {
                { typeof(IAccessoryType), localizer.Translate(TranslationType.ItemType, nameof(IAccessoryType)) },
                { typeof(IArmorType), localizer.Translate(TranslationType.ItemType, nameof(IArmorType)) },
                { typeof(IWeaponType), localizer.Translate(TranslationType.ItemType, nameof(IWeaponType)) },
                { typeof(IConsumerType), localizer.Translate(TranslationType.ItemType, nameof(IConsumerType)) },
                { typeof(ICustomGearType), localizer.Translate(TranslationType.ItemType, nameof(ICustomGearType)) },
            };

            _accessoryTypes = typeRegistry.GetRegisteredTypes<IAccessoryType>()
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _armorTypes = typeRegistry.GetRegisteredTypes<IArmorType>()
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _valuePoolTypes = typeRegistry.GetRegisteredTypes<IValuePoolType>()
                .Where(r => r.IsCraftable)
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _weaponTypes = typeRegistry.GetRegisteredTypes<IWeaponType>()
                .ToDictionary(x => x, x => localizer.Translate(x))
                .OrderByValue();

            _customGearTypes = typeRegistry.GetRegisteredTypes<ICustomGearType>()
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
