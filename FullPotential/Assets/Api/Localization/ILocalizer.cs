using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Localization
{
    public interface ILocalizer
    {
        Task LoadAvailableCulturesAsync(Dictionary<string, List<string>> localisationAddresses);

        Task LoadLocalizationFilesAsync(string culture);

        Dictionary<string, string> GetAvailableCultures();

        string Translate(string id);

        string Translate(TranslationType type, string suffix);

        string Translate(IRegisterable registeredItem);

        string Translate(Enum enumValue);
    }
}