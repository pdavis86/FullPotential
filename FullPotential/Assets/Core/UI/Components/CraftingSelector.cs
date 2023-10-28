using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Weapons;
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

        private List<string> _handednessOptions;
        private List<int?> _optionalTwoHandedWeaponIndexes;
        private List<string> _accessoryTypes;
        private List<string> _armorTypes;
        private List<string> _consumerTypes;
        private Dictionary<Type, string> _craftingCategories;
        private Dictionary<IWeapon, string> _weaponTypes;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            var localizer = DependenciesContext.Dependencies.GetService<ILocalizer>();

            _craftingCategories = new Dictionary<Type, string>
            {
                { typeof(Weapon), localizer.Translate(TranslationType.CraftingCategory, nameof(Weapon)) },
                { typeof(Armor), localizer.Translate(TranslationType.CraftingCategory, nameof(Armor)) },
                { typeof(Accessory), localizer.Translate(TranslationType.CraftingCategory, nameof(Accessory)) },
                { typeof(Consumer), localizer.Translate(TranslationType.CraftingCategory, nameof(Consumer)) }
            };

            _handednessOptions = new List<string> {
                localizer.Translate(TranslationType.CraftingHandedness, "one"),
                localizer.Translate(TranslationType.CraftingHandedness, "two")
            };

            var typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

            _armorTypes = Enum.GetValues(typeof(ArmorCategory))
                .Cast<ArmorCategory>()
                .Select(x => localizer.GetTranslatedTypeName(x))
                .OrderBy(x => x)
                .ToList();

            _accessoryTypes = Enum.GetValues(typeof(AccessoryCategory))
                .Cast<AccessoryCategory>()
                .Select(x => localizer.GetTranslatedTypeName(x))
                .OrderBy(x => x)
                .ToList();

            _consumerTypes = Enum.GetValues(typeof(ResourceConsumptionType))
                .Cast<ResourceConsumptionType>()
                .Select(x => localizer.GetTranslatedTypeName(x))
                .OrderBy(x => x)
                .ToList();

            _weaponTypes = typeRegistry.GetRegisteredTypes<IWeapon>()
                .ToDictionary(x => x, x => localizer.GetTranslatedTypeName(x))
                .OrderBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            _optionalTwoHandedWeaponIndexes = _weaponTypes
                .Select((x, i) => !x.Key.EnforceTwoHanded && x.Key.AllowTwoHanded ? (int?)i : null)
                .Where(x => x != null)
                .OrderBy(x => x)
                .ToList();

            TypeDropdown.ClearOptions();
            TypeDropdown.AddOptions(_craftingCategories.Select(x => x.Value).ToList());

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

        private void SetHandednessDropDownVisibility()
        {
            HandednessDropdown.gameObject.SetActive(GetCraftingCategory().Key == typeof(Weapon) && _optionalTwoHandedWeaponIndexes.Contains(SubTypeDropdown.value));
        }

        private void UpdateSecondaryDropDowns()
        {
            SubTypeDropdown.ClearOptions();

            var craftingCategory = GetCraftingCategory();
            switch (craftingCategory.Key.Name)
            {
                case nameof(Accessory): SubTypeDropdown.AddOptions(_accessoryTypes); break;
                case nameof(Armor): SubTypeDropdown.AddOptions(_armorTypes); break;
                case nameof(Weapon): SubTypeDropdown.AddOptions(_weaponTypes.Select(x => x.Value).ToList()); break;
                case nameof(Consumer): SubTypeDropdown.AddOptions(_consumerTypes); break;
                default: throw new InvalidOperationException($"Unknown crafting type: '{craftingCategory.Key.Name}'");
            }

            SubTypeDropdown.RefreshShownValue();
            SubTypeDropdown.gameObject.SetActive(true);

            SetHandednessDropDownVisibility();
        }

        public KeyValuePair<Type, string> GetCraftingCategory()
        {
            return _craftingCategories.ElementAt(TypeDropdown.value);
        }

        public string GetCraftableTypeName(string craftingCategory)
        {
            switch (craftingCategory)
            {
                case nameof(Accessory): return _accessoryTypes.ElementAt(SubTypeDropdown.value);
                case nameof(Armor): return _armorTypes.ElementAt(SubTypeDropdown.value);
                case nameof(Weapon): return _weaponTypes.ElementAt(SubTypeDropdown.value).Key.TypeName;
                case nameof(Consumer): return _consumerTypes.ElementAt(SubTypeDropdown.value);
                default: throw new InvalidOperationException($"Unknown crafting category: '{craftingCategory}'");
            }
        }

        public bool IsTwoHandedSelected()
        {
            return HandednessDropdown.options.Count > 0 && HandednessDropdown.value == 1;
        }
    }
}
