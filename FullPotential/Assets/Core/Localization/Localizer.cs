using System;
using FullPotential.Api.Registry;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Localization
{
    public class Localizer : ILocalizer
    {
        private readonly List<string> _addressesLoaded;
        private readonly Dictionary<string, string> _translations;
        private readonly List<CultureAddressables> _availableCultures;

        public const string DefaultCulture = "en-GB";

        public Localizer()
        {
            _addressesLoaded = new List<string>();
            _translations = new Dictionary<string, string>();
            _availableCultures = new List<CultureAddressables>();
        }

        private async Task<Data.Localization> LoadCultureFileAsync(string address)
        {
            try
            {
                var checkTask = Addressables.LoadResourceLocationsAsync(address).Task;
                await checkTask;

                if (checkTask.Result.Count == 0)
                {
                    throw new Exception($"Failed to find translations for '{address}'");
                }

                var loadTask = Addressables.LoadAssetAsync<TextAsset>(address).Task;
                await loadTask;

                var data = JsonUtility.FromJson<Data.Localization>(loadTask.Result.text);

                Addressables.Release(loadTask.Result);

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return default;
            }
        }

        private void ExtractTranslations(Data.Localization data, string address)
        {
            if (data.Translations == null)
            {
                Debug.LogError($"No translations found in addressable at '{address}'");
                return;
            }

            foreach (var item in data.Translations)
            {
                //todo: zzz v0.4.1 - translations should be prefixed with the mod name to prevent overwriting?

                if (_translations.ContainsKey(item.Key))
                {
                    Debug.LogWarning($"Translations already contains a value for key '{item.Key}'");
                }
                else
                {
                    _translations.Add(item.Key, item.Value);
                }
            }

            _addressesLoaded.Add(address);
        }

        public async Task LoadAvailableCulturesAsync(Dictionary<string, List<string>> localisationAddresses)
        {
            _availableCultures.Clear();

            foreach (var kvp in localisationAddresses)
            {
                var address = kvp.Value.FirstOrDefault(a => a.StartsWith("Core"))
                    ?? kvp.Value.First();

                var data = await LoadCultureFileAsync(address);

                _availableCultures.Add(new CultureAddressables
                {
                    Code = kvp.Key,
                    Name = data.Name.OrIfNullOrWhitespace(kvp.Key),
                    Addresses = kvp.Value
                });
            }
        }

        public async Task LoadLocalizationFilesAsync(string culture)
        {
            _addressesLoaded.Clear();
            _translations.Clear();

            var cultureMatch = _availableCultures.First(x => x.Code == culture);
            foreach (var address in cultureMatch.Addresses)
            {
                if (_addressesLoaded.Contains(address))
                {
                    //Debug.Log($"Skipping '{address}' because it is already loaded");
                    continue;
                }

                var data = await LoadCultureFileAsync(address);

                if (data.Translations == null)
                {
                    Debug.LogError($"Failed to load any translations from addressable at '{address}'");
                    continue;
                }

                ExtractTranslations(data, address);
            }
        }

        public Dictionary<string, string> GetAvailableCultures()
        {
            return _availableCultures.ToDictionary(x => x.Code, x => x.Name);
        }

        public string Translate(string id)
        {
            id = id.ToLower();

            if (_translations.ContainsKey(id))
            {
                return _translations[id];
            }

            Debug.LogWarning($"Missing translation for '{id}'");
            return $"'{id}' translation is missing";
        }

        public string Translate(IRegisterable registeredItem)
        {
            if (registeredItem is IWeapon) { return Translate("weapon." + registeredItem.TypeName); }
            if (registeredItem is ILoot) { return Translate("loot." + registeredItem.TypeName); }
            if (registeredItem is IEffect) { return Translate("effect." + registeredItem.TypeName); }
            if (registeredItem is IShape) { return Translate("shape." + registeredItem.TypeName); }
            if (registeredItem is ITargeting) { return Translate("targeting." + registeredItem.TypeName); }
            return "Unexpected IRegisterable type";
        }

        public string Translate(Enum enumValue)
        {
            return Translate(enumValue.GetType().Name + "." + enumValue);
        }

        public string Translate(TranslationType type, string suffix)
        {
            var split = SplitOnCapitals(type.ToString());
            var translationKey = string.Join('.', split) + '.' + suffix;
            return Translate(translationKey);
        }

        private static string[] SplitOnCapitals(string value)
        {
            const string regexSplitOnCapitals = @"(?<!^)(?=[A-Z])";
            return Regex.Split(value, regexSplitOnCapitals);
        }

    }
}
