using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Data
{
    [Serializable]
    public class Localization
    {
        public string Culture;
        public string Name;
        public KeyValuePair<string, string>[] Translations;
    }
}
