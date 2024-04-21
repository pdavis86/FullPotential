using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Localization
{
    public interface ILocalizer
    {
        public CultureInfo CurrentCulture { get; }

        Task LoadAvailableCulturesAsync(Dictionary<string, List<string>> localisationAddresses);

        Task LoadLocalizationFilesAsync(string cultureCode);

        Dictionary<string, string> GetAvailableCultures();

        string Translate(string id);

        string Translate(TranslationType type, string suffix);

        string Translate(IRegisterableType registeredItem);

        string Translate(string id, params object[] arguments);

        string Translate(int number);

        string Translate(float number, int decimalPlaces = 1);

        Dictionary<T, string> GetDictionaryFromEnum<T>(TranslationType translationType, bool sort = true) where T : Enum;
    }
}