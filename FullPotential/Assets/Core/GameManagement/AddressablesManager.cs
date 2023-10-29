using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace FullPotential.Core.GameManagement
{
    public class AddressablesManager
    {
        public List<string> ModPrefixes { get; }
        
        public Dictionary<string, List<string>> LocalisationAddresses { get; }

        public AddressablesManager()
        {
            ModPrefixes = new List<string>();
            LocalisationAddresses = new Dictionary<string, List<string>>();

            foreach (var resourceLocator in Addressables.ResourceLocators)
            {
                var stringKeys = resourceLocator.Keys.OfType<string>().ToList();

                FindModPrefixes(stringKeys);
                FindLocalisationFiles(stringKeys);
            }
        }

        private void FindModPrefixes(IEnumerable<string> stringKeys)
        {
            const string suffix = "/Registration";

            var modRegistrationAddresses = stringKeys.Where(x =>  x.EndsWith(suffix));

            if (!modRegistrationAddresses.Any())
            {
                return;
            }

            var groupedByMod = modRegistrationAddresses.GroupBy(a =>
            {
                var removedSuffix = a.Substring(0, a.Length - suffix.Length);
                var removedPrefix = removedSuffix.Substring(removedSuffix.LastIndexOf("/", StringComparison.Ordinal) + 1);
                return removedPrefix;
            });

            foreach (var modGroup in groupedByMod)
            {
                if (modGroup.Key == "Core")
                {
                    continue;
                }

                ModPrefixes.Add(modGroup.Key);
            }
        }

        private void FindLocalisationFiles(IEnumerable<string> stringKeys)
        {
            var localisationAddresses = stringKeys
                .Where(x => x.Contains("/Localization/") && x.EndsWith(".json"))
                .ToList();

            if (!localisationAddresses.Any())
            {
                return;
            }

            var groupedByLanguage = localisationAddresses.GroupBy(System.IO.Path.GetFileNameWithoutExtension);
            foreach (var languageGroup in groupedByLanguage)
            {
                LocalisationAddresses.Add(languageGroup.Key, languageGroup.ToList());
            }
        }
    }
}
