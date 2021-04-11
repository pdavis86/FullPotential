using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Core.Localization
{
    public class Localizer
    {
        private Dictionary<string, string> _translations;

        private Localizer() { }

        private static Localizer _instance = new Localizer();
        public static Localizer Instance { get { return _instance; } }

        public void LoadLocalizationFile(string culture = null)
        {
            var dataDir = Path.Combine(Application.dataPath, "Core/Localization");
            string filePath = Path.Combine(dataDir, culture + ".json");

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

            _translations = data.GetDictionary();
        }

        public string Translate(string id)
        {
            if (_translations.ContainsKey(id))
            {
                return _translations[id];
            }

            Debug.LogWarning($"Missing translation for '{id}'");
            return "**MISSING TRANSLATION**";
        }

        public string TranslateWithFallback(string id, string fallback)
        {
            if (_translations.ContainsKey(id))
            {
                return _translations[id];
            }
            return fallback;
        }

    }
}
