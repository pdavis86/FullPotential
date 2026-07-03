using System;

using FullPotential.Api.Obsolete;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Localization.Models
{
    // todo: zzz v0.6 - kill LocalizationOld
    [Serializable]
    public struct LocalizationOld
    {
        public string Culture;
        public string Name;
        public SerializableKeyValuePair<string, string>[] Translations;
    }
}
