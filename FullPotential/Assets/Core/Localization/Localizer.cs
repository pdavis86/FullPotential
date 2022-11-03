using FullPotential.Api.Registry;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Localization
{
    public class Localizer : ILocalizer
    {
        private Dictionary<string, string> _translations;
        private List<CultureAddressables> _availableCultures;

        public const string DefaultCulture = "en-GB";

        private async Task<Data.Localization> LoadCultureFileAsync(string address)
        {
            var checkTask = Addressables.LoadResourceLocationsAsync(address).Task;
            await checkTask;

            if (checkTask.Result.Count == 0)
            {
                //NOTE: Failure to find the file causes the app to hang
                throw new System.Exception($"Failed to find translations for '{address}'");
            }

            var loadTask = Addressables.LoadAssetAsync<TextAsset>(address).Task;
            await loadTask;
            var data = JsonUtility.FromJson<Data.Localization>(loadTask.Result.text);
            Addressables.Release(loadTask.Result);

            return data;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public async Task LoadAvailableCulturesAsync(Dictionary<string, List<string>> localisationAddresses)
        {
            _availableCultures = new List<CultureAddressables>();
            foreach (var kvp in localisationAddresses)
            {
                var data = await LoadCultureFileAsync(kvp.Value.First());
                _availableCultures.Add(new CultureAddressables
                {
                    Code = kvp.Key,
                    Name = data.Name.OrIfNullOrWhitespace(kvp.Key),
                    Addresses = kvp.Value
                });
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public async Task LoadLocalizationFilesAsync(string culture)
        {
            _translations = new Dictionary<string, string>();

            var cultureMatch = _availableCultures.First(x => x.Code == culture);
            foreach (var address in cultureMatch.Addresses)
            {
                var data = await LoadCultureFileAsync(address);

                if (data.Translations == null)
                {
                    throw new System.Exception($"Failed to load any translations for address '{address}'");
                }

                foreach (var item in data.Translations)
                {
                    if (_translations.ContainsKey(item.Key))
                    {
                        Debug.LogWarning($"Translations already contains a value for key '{item.Key}'");
                    }
                    else
                    {
                        _translations.Add(item.Key, item.Value);
                    }
                }
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
            return $"{id} translation is missing";
        }

        public string GetTranslatedTypeName(IRegisterable registeredItem)
        {
            if (registeredItem is IGearAccessory) { return Translate("accessory." + registeredItem.TypeName); }
            if (registeredItem is IGearArmor) { return Translate("armor." + registeredItem.TypeName); }
            if (registeredItem is IGearWeapon) { return Translate("weapon." + registeredItem.TypeName); }
            if (registeredItem is ILoot) { return Translate("loot." + registeredItem.TypeName); }
            if (registeredItem is IEffect) { return Translate("effect." + registeredItem.TypeName); }
            if (registeredItem is IShape) { return Translate("shape." + registeredItem.TypeName); }
            if (registeredItem is ITargeting) { return Translate("targeting." + registeredItem.TypeName); }
            return "Unexpected ICraftable type";
        }

        private static string[] SplitOnCapitals(string value)
        {
            const string regexSplitOnCapitals = @"(?<!^)(?=[A-Z])";
            return Regex.Split(value, regexSplitOnCapitals);
        }

        public string Translate(TranslationType type, string suffix)
        {
            var split = SplitOnCapitals(type.ToString());
            var translationKey = string.Join('.', split) + '.' + suffix;
            return Translate(translationKey);
        }

    }
}
