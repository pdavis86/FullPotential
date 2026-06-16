using System;
using System.Collections.Generic;

using FullPotential.Api.Data;
using FullPotential.Api.Localization;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Items.Base
{
    // todo: zzz v0.6 - remove Serializable
    // todo: zzz v0.6 - make abstract
    [Serializable]
    public class ItemBase : ISaveable
    {
        public string Id;
        public string CharacterId;
        public string RegistryTypeId;
        public string Name;

        public Dictionary<string, string> AttributeDictionary { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> PropertyDictionary { get; } = new Dictionary<string, string>();

        public List<string> EffectIdEnumerable { get; } = new List<string>();

        public bool IsDeleted { get; set; }

        public bool IsDirty { get; set; }

        private IRegisterableType _registryType;
        public IRegisterableType RegistryType
        {
            get => _registryType;
            set
            {
                _registryType = value;
                RegistryTypeId = _registryType.TypeId.ToString();
            }
        }

        public virtual string GetName(ILocalizer localizer)
        {
            return Name;
        }

        public virtual string GetDescription(ILocalizer localizer, LevelOfDetail levelOfDetail = LevelOfDetail.Full, string itemName = null)
        {
            return levelOfDetail == LevelOfDetail.Full ? Name : null;
        }
    }
}
