using System.Collections.Generic;
using System.Threading.Tasks;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Localization
{
    public interface ILocalizer
    {
        Task<bool> LoadAvailableCulturesAsync();

        Task<bool> LoadLocalizationFilesAsync(string culture);

        Dictionary<string, string> GetAvailableCultures();

        string Translate(string id);

        string Translate(TranslationType type, string suffix);

        string GetTranslatedTypeName(IRegisterable registeredItem);
    }
}