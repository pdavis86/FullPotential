using System;
using FullPotential.Api.Registry;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
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
        public const string DefaultCulture = "en-GB";

        private readonly List<string> _addressesLoaded = new List<string>();
        private readonly Dictionary<string, string> _translations = new Dictionary<string, string>();
        private readonly List<CultureAddressables> _availableCultures = new List<CultureAddressables>();
        private readonly Dictionary<Type, string> _typeDictionary = new Dictionary<Type, string>();

        public CultureInfo CurrentCulture { get; private set; }

        public Localizer()
        {
            CacheRegisterableTypeNames();
        }

        private void CacheRegisterableTypeNames()
        {
            _typeDictionary.Clear();

            CacheRegisterableTypeName<IResource>();
            CacheRegisterableTypeName<IArmor>();
            CacheRegisterableTypeName<IAccessory>();
            CacheRegisterableTypeName<IWeapon>();
            CacheRegisterableTypeName<ILoot>();
            CacheRegisterableTypeName<IAmmunition>();
            CacheRegisterableTypeName<IEffect>();
            CacheRegisterableTypeName<IShape>();
            CacheRegisterableTypeName<ITargeting>();
            CacheRegisterableTypeName<ISpecialGear>();

            //NOTE: Add this last to catch anything missed
            _typeDictionary.Add(typeof(IRegisterableWithSlot), "slot");
        }

        private void CacheRegisterableTypeName<T>()
        {
            var interfaceType = typeof(T);

            if (!interfaceType.Name.StartsWith("I"))
            {
                throw new Exception("Was expecting an interface");
            }

            _typeDictionary.Add(interfaceType, interfaceType.Name.Substring(1).ToLower());
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
                //todo: zzz v0.4 - translations should be prefixed with the mod name to prevent overwriting?

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

        public async Task LoadLocalizationFilesAsync(string cultureCode)
        {
            _addressesLoaded.Clear();
            _translations.Clear();

            CurrentCulture = new CultureInfo(cultureCode);

            var cultureMatch = _availableCultures.First(x => x.Code == cultureCode);
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

            if (_translations.TryGetValue(id, out var translation))
            {
                return translation;
            }

            Debug.LogWarning($"Missing translation for '{id}'");
            return $"'{id}' translation is missing";
        }

        public string Translate(IRegisterable registeredItem)
        {
            foreach (var kvp in _typeDictionary)
            {
                if (kvp.Key.IsInstanceOfType(registeredItem))
                {
                    return Translate(kvp.Value + "." + registeredItem.GetType().Name);
                }
            }

            return "Unexpected IRegisterable type";
        }

        public string Translate(Enum enumValue)
        {
            return Translate(enumValue.GetType().Name + "." + enumValue);
        }

        public string TranslateWithArgs(string id, params object[] arguments)
        {
            return string.Format(Translate(id), arguments);
        }

        public string TranslateInt(int input)
        {
            return input.ToString();
        }

        public string TranslateFloat(float input, int decimalPlaces = 1)
        {
            var rounded = Math.Round(input, decimalPlaces);
            return rounded.ToString(CurrentCulture);
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
