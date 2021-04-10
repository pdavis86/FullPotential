using Assets.ApiScripts.Crafting;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable UseFormatSpecifierInInterpolation

namespace Assets.Core.Crafting
{
    [System.Serializable]
    public abstract class ItemBase
    {
        public string Id;
        public string Name;
        public Attributes Attributes;
        public string[] EffectIds;


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


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 101;
                hash = hash * 103 + Id.GetHashCode();
                hash = hash * 107 + Name.GetHashCode();
                hash = hash * 109 + Attributes.GetHashCode();
                hash = hash * 113 + (EffectIds != null ? string.Join(null, EffectIds) : string.Empty).GetHashCode();
                return hash;
            }
        }

        public string GetFullName()
        {
            if (this is Loot)
            {
                var suffix = int.Parse(GetHashCode().ToString().TrimStart('-').Substring(5));
                return Name + $" (Type #{suffix.ToString("D5")})";
            }
            return Name;
        }

    }
}
