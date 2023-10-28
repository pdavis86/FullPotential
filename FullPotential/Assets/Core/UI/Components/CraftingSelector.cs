using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
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
        // ReSharper restore UnassignedField.Global

        private Dictionary<CraftableType, string> _craftingTypes;
        private Dictionary<string, string> _accessoryTypes;
        private Dictionary<string, string> _armorTypes;
        private Dictionary<string, string> _consumerTypes;
        private Dictionary<IWeapon, string> _weaponTypes;
        private List<string> _handednessOptions;
        private List<int?> _optionalTwoHandedWeaponIndexes;

        #region "Event Handlers"

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            FillDataVariables();

            TypeDropdown.ClearOptions();
            TypeDropdown.AddOptions(_craftingTypes.Select(x => x.Value).ToList());

            HandednessDropdown.ClearOptions();
            HandednessDropdown.AddOptions(_handednessOptions);

            TypeDropdown.onValueChanged.AddListener(TypeOnValueChanged);

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
            return _craftingTypes.ElementAt(TypeDropdown.value).Key;
        }

        public string GetSubTypeName(CraftableType craftableType)
        {
            switch (craftableType)
            {
                case CraftableType.Accessory: return _accessoryTypes.ElementAt(SubTypeDropdown.value).Key;
                case CraftableType.Armor: return _armorTypes.ElementAt(SubTypeDropdown.value).Key;
                case CraftableType.Weapon: return _weaponTypes.ElementAt(SubTypeDropdown.value).Key.TypeName;
                case CraftableType.Consumer: return _consumerTypes.ElementAt(SubTypeDropdown.value).Key;
                default: throw new InvalidOperationException($"Unknown crafting category: '{craftableType}'");
            }
        }

        public bool IsTwoHandedSelected()
        {
            return HandednessDropdown.options.Count > 0 && HandednessDropdown.value == 1;
        }

        private void SetHandednessDropDownVisibility()
        {
            HandednessDropdown.gameObject.SetActive(GetTypeToCraft() == CraftableType.Weapon && _optionalTwoHandedWeaponIndexes.Contains(SubTypeDropdown.value));
        }

        private void UpdateSecondaryDropDowns()
        {
            SubTypeDropdown.ClearOptions();

            var typeToCraft = GetTypeToCraft();
            switch (typeToCraft)
            {
                case CraftableType.Accessory: SubTypeDropdown.AddOptions(_accessoryTypes.Select(x => x.Value).ToList()); break;
                case CraftableType.Armor: SubTypeDropdown.AddOptions(_armorTypes.Select(x => x.Value).ToList()); break;
                case CraftableType.Weapon: SubTypeDropdown.AddOptions(_weaponTypes.Select(x => x.Value).ToList()); break;
                case CraftableType.Consumer: SubTypeDropdown.AddOptions(_consumerTypes.Select(x => x.Value).ToList()); break;
                default: throw new InvalidOperationException($"Unknown crafting type: '{typeToCraft}'");
            }

            SubTypeDropdown.RefreshShownValue();
            SubTypeDropdown.gameObject.SetActive(true);

            SetHandednessDropDownVisibility();
        }

        private void FillDataVariables()
        {
            var localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();
            var typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

            _craftingTypes = Enum.GetValues(typeof(CraftableType))
                .Cast<CraftableType>()
                .ToDictionary(x => x, x => localizer.Translate(x));

            _accessoryTypes = GetDictionaryFromEnum<AccessoryCategory>(localizer);

            _armorTypes = GetDictionaryFromEnum<ArmorCategory>(localizer);

            _consumerTypes = GetDictionaryFromEnum<ResourceConsumptionType>(localizer);

            _weaponTypes = typeRegistry.GetRegisteredTypes<IWeapon>()
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

        private Dictionary<string, string> GetDictionaryFromEnum<T>(ILocalizer localizer, bool sort = true)
            where T : Enum
        {
            var dictionary = Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(x => x.ToString(), x => localizer.Translate(x));

            return sort
                ? dictionary.OrderByValue()
                : dictionary;
        }
    }
}
