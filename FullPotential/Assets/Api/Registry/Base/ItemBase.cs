using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Api.Registry.Base
{
    [System.Serializable]
    public abstract class ItemBase
    {
        public string Id;
        public string RegistryTypeId;
        public string Name;
        public Attributes Attributes;
        public string[] EffectIds;
        public bool IsTwoHanded;

        private List<IEffect> _effects;
        public List<IEffect> Effects
        {
            get
            {
                return _effects;
            }
            set
            {
                _effects = value;
                EffectIds = _effects.Select(x => x.TypeId.ToString()).ToArray();
            }
        }

        private IRegisterable _registryType;
        public IRegisterable RegistryType
        {
            get
            {
                return _registryType;
            }
            set
            {
                _registryType = value;
                RegistryTypeId = _registryType.TypeId.ToString();
            }
        }

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 101;
                hash = hash * 103 + Id.GetHashCode();
                hash = hash * 107 + (RegistryTypeId ?? string.Empty).GetHashCode();
                hash = hash * 109 + (Name ?? string.Empty).GetHashCode();
                hash = hash * 113 + Attributes.GetHashCode();
                hash = hash * 127 + (EffectIds != null ? string.Join(null, EffectIds) : string.Empty).GetHashCode();
                return hash;
            }
        }

    }
}
