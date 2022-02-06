using System;
using FullPotential.Api.Data;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Data
{
    [Serializable]
    public struct Localization
    {
        public string Culture;
        public string Name;
        public KeyValuePair<string, string>[] Translations;
    }
}
