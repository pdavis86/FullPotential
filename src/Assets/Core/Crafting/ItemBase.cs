﻿using System.Collections.Generic;

// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable UseFormatSpecifierInInterpolation

namespace Assets.Core.Crafting
{
    //todo: can I make this an abstract class?

    [System.Serializable]
    public class ItemBase
    {
        public const string LootPrefixScrap = "Scrap";
        public const string LootPrefixShard = "Shard";

        public string Id;
        public string Name;
        public Attributes Attributes;
        public List<string> Effects;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 101;
                hash = hash * 103 + Id.GetHashCode();
                hash = hash * 107 + Name.GetHashCode();
                hash = hash * 109 + Attributes.GetHashCode();
                hash = hash * 113 + string.Join(null, Effects).GetHashCode();
                return hash;
            }
        }

        public string GetFullName()
        {
            if (Name == LootPrefixScrap || Name == LootPrefixShard)
            {
                var suffix = int.Parse(GetHashCode().ToString().TrimStart('-').Substring(5));
                return Name + $" (Type #{suffix.ToString("D5")})";
            }
            return Name;
        }

    }
}
