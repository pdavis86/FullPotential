using Assets.ApiScripts.Registry;
using Assets.Core.Spells.Shapes;
using Assets.Core.Spells.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

namespace Assets.Core.Localization
{
    public class Localizer
    {
        private Dictionary<string, string> _translations;

        public void LoadLocalizationFiles(string culture = null, IEnumerable<string> modFilePaths = null)
        {
            IEnumerable<string> filepaths = new[] { "Core/Localization" };

            if (modFilePaths != null && modFilePaths.Any())
            {
                filepaths = filepaths.Union(modFilePaths);
            }

            _translations = new Dictionary<string, string>();

            foreach (var relativePath in filepaths)
            {
                var dataDir = Path.Combine(Application.dataPath, relativePath);
                var filePath = Path.Combine(dataDir, culture + ".json");

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Failed to find translations for culture '{culture}'. Defaulting to 'en-GB'");

                    culture = "en-GB";
                    filePath = Path.Combine(dataDir, culture + ".json");

                    if (!File.Exists(filePath))
                    {
                        throw new Exception("Failed to find any localization file");
                    }
                }

                var data = JsonUtility.FromJson<Assets.Core.Data.Localization>(File.ReadAllText(filePath));
                foreach (var item in data.Translations)
                {
                    if (!_translations.ContainsKey(item.Key))
                    {
                        _translations.Add(item.Key, item.Value);
                    }
                    else
                    {
                        Debug.LogWarning($"Translations already contains a value for key '{item.Key}'");
                    }
                }
            }
        }

        public string Translate(string id)
        {
            id = id.ToLower();

            if (_translations.ContainsKey(id))
            {
                return _translations[id];
            }

            Debug.LogWarning($"Missing translation for '{id}'");
            return $"{id} translation is missing";
        }

        public string GetTranslatedTypeName(IRegisterable registeredItem)
        {
            if (registeredItem is IGearAccessory) { return Translate("accessory." + registeredItem.TypeName); }
            if (registeredItem is IGearArmor) { return Translate("armor." + registeredItem.TypeName); }
            if (registeredItem is IGearWeapon) { return Translate("weapon." + registeredItem.TypeName); }
            if (registeredItem is ILoot) { return Translate("loot." + registeredItem.TypeName); }
            if (registeredItem is IEffect) { return Translate("effect." + registeredItem.TypeName); }
            if (registeredItem is ISpellShape) { return Translate("spell.shape." + registeredItem.TypeName); }
            if (registeredItem is ISpellTargeting) { return Translate("spell.targeting." + registeredItem.TypeName); }
            return "Unexpected ICraftable type";
        }

        public enum TranslationType
        {
            CraftingCategory,
            CraftingNamePrefix,
            WeaponHandedness,
            Attribute
        }

        public string Translate(TranslationType type, string suffix)
        {
            switch (type)
            {
                case TranslationType.CraftingCategory: return Translate("crafting.category." + suffix);
                case TranslationType.CraftingNamePrefix: return Translate("crafting.name.prefix." + suffix);
                case TranslationType.WeaponHandedness: return Translate("crafting.handedness." + suffix);
                case TranslationType.Attribute: return Translate("attribute." + suffix);
                default: return "Unexpected translation type";
            }
        }

    }
}
