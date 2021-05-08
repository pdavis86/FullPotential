using Assets.ApiScripts.Registry;
using Assets.Core.Spells.Shapes;
using Assets.Core.Spells.Targeting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Assets.Core.Localization
{
    public class Localizer
    {
        private readonly IEnumerable<string> _modPathStems;
        private Dictionary<string, string> _translations;

        public Localizer(IEnumerable<string> modPathStems)
        {
            _modPathStems = modPathStems;
        }

        public List<string> GetAvailableCultures()
        {
            var cultures = new List<string>();
            foreach (var rl in Addressables.ResourceLocators)
            {
                cultures.AddRange(rl.Keys
                    .OfType<string>()
                    .Where(x => x.StartsWith("Core/Localization/") && x.EndsWith(".json"))
                    .Select(System.IO.Path.GetFileNameWithoutExtension)
                    );
            }
            return cultures;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public async Task<bool> LoadLocalizationFiles(string culture)
        {
            _translations = new Dictionary<string, string>();

            foreach (var modName in _modPathStems)
            {
                var address = $"{modName}/Localization/{culture}.json";

                var checkTask = Addressables.LoadResourceLocationsAsync(address).Task;
                await checkTask;

                if (checkTask.Result.Count == 0)
                {
                    Debug.LogError($"Failed to find translations for '{address}'");
                    continue;
                }

                var loadTask = Addressables.LoadAssetAsync<TextAsset>(address).Task;
                await loadTask;
                var data = JsonUtility.FromJson<Assets.Core.Data.Localization>(loadTask.Result.text);
                foreach (var item in data.Translations)
                {
                    if (_translations.ContainsKey(item.Key))
                    {
                        Debug.LogWarning($"Translations already contains a value for key '{item.Key}'");
                    }
                    else
                    {
                        _translations.Add(item.Key, item.Value);
                    }
                }

                Addressables.Release(loadTask.Result);
            }

            return true;
        }

        public string Translate(string id)
        {
            id = id.ToLower();

            if (_translations.ContainsKey(id))
            {
                return _translations[id];
            }

            Debug.LogWarning($"Missing translation for '{id}'");
            return $"{id} translation is missing";
        }

        public string GetTranslatedTypeName(IRegisterable registeredItem)
        {
            if (registeredItem is IGearAccessory) { return Translate("accessory." + registeredItem.TypeName); }
            if (registeredItem is IGearArmor) { return Translate("armor." + registeredItem.TypeName); }
            if (registeredItem is IGearWeapon) { return Translate("weapon." + registeredItem.TypeName); }
            if (registeredItem is ILoot) { return Translate("loot." + registeredItem.TypeName); }
            if (registeredItem is IEffect) { return Translate("effect." + registeredItem.TypeName); }
            if (registeredItem is ISpellShape) { return Translate("spell.shape." + registeredItem.TypeName); }
            if (registeredItem is ISpellTargeting) { return Translate("spell.targeting." + registeredItem.TypeName); }
            return "Unexpected ICraftable type";
        }

        public enum TranslationType
        {
            CraftingCategory,
            CraftingNamePrefix,
            WeaponHandedness,
            Attribute
        }

        public string Translate(TranslationType type, string suffix)
        {
            switch (type)
            {
                case TranslationType.CraftingCategory: return Translate("crafting.category." + suffix);
                case TranslationType.CraftingNamePrefix: return Translate("crafting.name.prefix." + suffix);
                case TranslationType.WeaponHandedness: return Translate("crafting.handedness." + suffix);
                case TranslationType.Attribute: return Translate("attribute." + suffix);
                default: return "Unexpected translation type";
            }
        }

    }
}
