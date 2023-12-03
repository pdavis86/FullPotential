using System;
using FullPotential.Api.Data;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Localization.Data
{
    [Serializable]
    public struct Localization
    {
        public string Culture;
        public string Name;
        public SerializableKeyValuePair<string, string>[] Translations;
    }
}
