using System.Collections.Generic;

namespace FullPotential.Core.Localization.Models
{
    public struct LocalizationData
    {
        public string Culture;
        public string Name;
        public Dictionary<string, string> Translations;
    }
}
