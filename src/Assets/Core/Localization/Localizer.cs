using Assets.ApiScripts.Crafting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Core.Localization
{
    public class Localizer
    {
        private Dictionary<string, string> _translations;

        private Localizer() { }

        private static Localizer _instance = new Localizer();
        public static Localizer Instance { get { return _instance; } }

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
            return "Unexpected ICraftable type";
        }

    }
}
