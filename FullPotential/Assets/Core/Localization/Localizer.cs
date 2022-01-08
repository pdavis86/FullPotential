using FullPotential.Api.Registry;
using FullPotential.Core.Extensions;
using FullPotential.Core.Spells.Shapes;
using FullPotential.Core.Spells.Targeting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace FullPotential.Core.Localization
{
    public class Localizer
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
        public async Task<bool> LoadAvailableCulturesAsync()
        {
            var localisationAddresses = new Dictionary<string, List<string>>();

            foreach (var rl in Addressables.ResourceLocators)
            {
                var addresses = rl.Keys
                    .OfType<string>()
                    .Where(x => x.Contains("/Localization/") && x.EndsWith(".json"));

                if (addresses.Any())
                {
                    var groups = addresses.GroupBy(System.IO.Path.GetFileNameWithoutExtension);
                    foreach (var grouping in groups)
                    {
                        localisationAddresses.Add(grouping.Key, grouping.ToList());
                    }
                }
            }

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

            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public async Task<bool> LoadLocalizationFilesAsync(string culture)
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

            return true;
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
