using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Core.GameManagement;
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
        // ReSharper enable UnassignedField.Global

        private List<string> _handednessOptions;
        private List<int?> _optionalTwoHandedWeaponIndexes;
        private Dictionary<Type, string> _craftingCategories;
        private Dictionary<IGearArmor, string> _armorTypes;
        private Dictionary<IGearAccessory, string> _accessoryTypes;
        private Dictionary<IGearWeapon, string> _weaponTypes;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            var localizer = GameManager.Instance.GetService<ILocalizer>();

            _craftingCategories = new Dictionary<Type, string>
            {
                { typeof(Weapon), localizer.Translate(TranslationType.CraftingCategory, nameof(Weapon)) },
                { typeof(Armor), localizer.Translate(TranslationType.CraftingCategory, nameof(Armor)) },
                { typeof(Accessory), localizer.Translate(TranslationType.CraftingCategory, nameof(Accessory)) },
                { typeof(Spell), localizer.Translate(TranslationType.CraftingCategory, nameof(Spell)) },
                { typeof(Gadget), localizer.Translate(TranslationType.CraftingCategory, nameof(Gadget)) }
            };

            _handednessOptions = new List<string> {
                localizer.Translate(TranslationType.CraftingHandedness, "one"),
                localizer.Translate(TranslationType.CraftingHandedness, "two")
            };

            var typeRegistry = GameManager.Instance.GetService<ITypeRegistry>();

            _armorTypes = typeRegistry.GetRegisteredTypes<IGearArmor>()
                .ToDictionary(x => x, x => localizer.GetTranslatedTypeName(x))
                .ToDictionary(x => x.Key, x => x.Value);

            _accessoryTypes = typeRegistry.GetRegisteredTypes<IGearAccessory>()
                .ToDictionary(x => x, x => localizer.GetTranslatedTypeName(x))
                .OrderBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            _weaponTypes = typeRegistry.GetRegisteredTypes<IGearWeapon>()
                .ToDictionary(x => x, x => localizer.GetTranslatedTypeName(x))
                .OrderBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            _optionalTwoHandedWeaponIndexes = _weaponTypes
                .Select((x, i) => !x.Key.EnforceTwoHanded && x.Key.AllowTwoHanded ? (int?)i : null)
                .Where(x => x != null)
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

            var shownSubTypes = true;

            var craftingCategory = GetCraftingCategory();
            switch (craftingCategory.Key.Name)
            {
                case nameof(Weapon): SubTypeDropdown.AddOptions(_weaponTypes.Select(x => x.Value).ToList()); break;
                case nameof(Armor): SubTypeDropdown.AddOptions(_armorTypes.Select(x => x.Value).ToList()); break;
                case nameof(Accessory): SubTypeDropdown.AddOptions(_accessoryTypes.Select(x => x.Value).ToList()); break;

                case nameof(Spell):
                case nameof(Gadget):
                    shownSubTypes = false;
                    break;

                default:
                    throw new InvalidOperationException("Unknown crafting type");
            }

            if (shownSubTypes)
            {
                SubTypeDropdown.RefreshShownValue();
                SubTypeDropdown.gameObject.SetActive(true);
            }
            else
            {
                SubTypeDropdown.gameObject.SetActive(false);
            }

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
                case nameof(Weapon): return _weaponTypes.ElementAt(SubTypeDropdown.value).Key.TypeName;
                case nameof(Armor): return _armorTypes.ElementAt(SubTypeDropdown.value).Key.TypeName;
                case nameof(Accessory): return _accessoryTypes.ElementAt(SubTypeDropdown.value).Key.TypeName;

                case nameof(Spell):
                case nameof(Gadget):
                    return null;

                default: throw new InvalidOperationException("Unknown crafting type");
            }
        }

        public bool IsTwoHandedSelected()
        {
            return HandednessDropdown.options.Count > 0 && HandednessDropdown.value == 1;
        }
    }
}
