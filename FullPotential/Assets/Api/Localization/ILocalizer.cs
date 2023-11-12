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

        string Translate(IRegisterable registeredItem);

        string Translate(Enum enumValue);

        string TranslateWithArgs(string id, params object[] arguments);

        string TranslateInt(int number);

        string TranslateFloat(float number, int decimalPlaces = 1);
    }
}