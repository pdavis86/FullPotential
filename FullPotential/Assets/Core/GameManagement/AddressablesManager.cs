using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace FullPotential.Core.GameManagement
{
    public class AddressablesManager
    {
        public Dictionary<string, List<string>> LocalisationAddresses { get; }

        public List<string> ModPrefixes { get; }

        public AddressablesManager()
        {
            LocalisationAddresses = new Dictionary<string, List<string>>();
            ModPrefixes = new List<string>();

            foreach (var rl in Addressables.ResourceLocators)
            {
                var localisationAddresses = rl.Keys
                    .OfType<string>()
                    .Where(x => x.Contains("/Localization/") && x.EndsWith(".json"));

                if (!localisationAddresses.Any())
                {
                    continue;
                }

                var groupedByMod = localisationAddresses.GroupBy(a => a.Substring(0, a.IndexOf("/", StringComparison.Ordinal)));
                foreach (var modGroup in groupedByMod)
                {
                    if (modGroup.Key == "Core")
                    {
                        continue;
                    }

                    ModPrefixes.Add(modGroup.Key);
                }

                var groupedByLanguage = localisationAddresses.GroupBy(System.IO.Path.GetFileNameWithoutExtension);
                foreach (var languageGroup in groupedByLanguage)
                {
                    LocalisationAddresses.Add(languageGroup.Key, languageGroup.ToList());
                }
            }
        }
    }
}
