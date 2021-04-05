using Assets.ApiScripts.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable PossibleMultipleEnumeration

namespace Assets.Core.Crafting
{
    public class ResultFactory
    {
        //todo: remove these in favour of the Assets.ApiScripts.Crafting.ICraftable.CraftingCategory values
        public const string CraftingTypeWeapon = "Weapon";
        public const string CraftingTypeArmor = "Armor";
        public const string CraftingTypeAccessory = "Accessory";
        public const string CraftingTypeSpell = "Spell";

        // ReSharper disable once InconsistentNaming
        private static readonly Random _random = new Random();

        private int ComputeAttribute(IEnumerable<ItemBase> components, Func<ItemBase, int> getProp, bool allowMax = true)
        {
            var withValue = components.Where(x => getProp(x) > 0);

            if (!withValue.Any())
            {
                return 1;
            }

            var min = withValue.Min(getProp);
            var max = withValue.Max(getProp);
            var topEndSkew = max - ((max - min) / 10);

            int result = allowMax
                ? topEndSkew
                : (int)Math.Round(topEndSkew - (0.009 * (topEndSkew - 50)), MidpointRounding.AwayFromZero);

            //Debug.Log($"{getProp.Method.Name} = Min:{min}, Max:{max}, Skew:{topEndSkew}, Result:{result}");

            return result == 0 ? 1 : result;
        }

        private string GetTargeting(IEnumerable<string> effectsInput)
        {
            //Only one target
            var target = effectsInput.Intersect(Spell.TargetingOptions.All).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(target))
            {
                return target;
            }

            return Spell.TargetingOptions.Projectile;
        }

        private string GetShape(string targeting, IEnumerable<string> effectsInput)
        {
            //Only one shape
            if (targeting != Spell.TargetingOptions.Beam && targeting != Spell.TargetingOptions.Cone)
            {
                var shape = effectsInput.Intersect(Spell.ShapeOptions.All).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(shape))
                {
                    return shape;
                }
            }

            return null;
        }

        private List<string> GetEffects(string craftingType, IEnumerable<string> effectsInput)
        {
            var effects = effectsInput
                .Except(new[] { Spell.BuffEffects.LifeTap, Spell.BuffEffects.ManaTap })
                .Except(Spell.TargetingOptions.All)
                .Except(Spell.ShapeOptions.All);

            var elementalEffects = effects.Intersect(Spell.ElementalEffects.All);
            var elementalEffect = elementalEffects.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(elementalEffect))
            {
                effects = effects
                    .Except(Spell.ElementalEffects.All.Where(x => x != elementalEffect));
            }

            var lingeringEffect = Spell.LingeringPairing.FirstOrDefault(x => x.Key == elementalEffect).Value;
            effects = effects.Except(Spell.LingeringOptions.All.Where(x => x != lingeringEffect));

            if (craftingType == CraftingTypeArmor || craftingType == CraftingTypeAccessory)
            {
                return effects.Intersect(Spell.BuffEffects.All)
                    .Union(effects.Intersect(Spell.SupportEffects.All))
                    .Union(effects.Intersect(Spell.ElementalEffects.All))
                    .ToList();
            }

            if (craftingType == CraftingTypeWeapon)
            {
                return effects.Intersect(Spell.DebuffEffects.All)
                    .Union(effects.Intersect(Spell.ElementalEffects.All))
                    .Union(effects.Intersect(Spell.LingeringOptions.All))
                    .ToList();
            }

            if (craftingType != CraftingTypeSpell)
            {
                throw new Exception($"Unexpected craftingType '{craftingType}'");
            }

            if (effects.Intersect(Spell.BuffEffects.All).Any() || effects.Intersect(Spell.SupportEffects.All).Any())
            {
                effects = effects
                    .Except(Spell.DebuffEffects.All)
                    .Except(Spell.ElementalEffects.All);
            }

            return effects.ToList();
        }

        private int GetAttributeValueIfRandomAbove(int rarityThreshold)
        {
            return _random.Next(0, 100) > rarityThreshold ? _random.Next(1, 100) : 0;
        }

        //private int PickValueAtRandom(IEnumerable<ItemBase> components, Func<ItemBase, int> getProp)
        //{
        //    var values = components.Select(getProp).ToList();
        //    var takeAt = _random.Next(0, values.Count - 1);
        //    return values.ElementAt(takeAt);
        //}

        private Attributes GetRandomAttributes()
        {
            return new Attributes
            {
                IsAutomatic = _random.Next(0, 100) > 50,
                IsSoulbound = _random.Next(0, 100) > 90,
                ExtraAmmoPerShot = _random.Next(0, 100) > 70 ? _random.Next(1, 3) : 0,
                Strength = GetAttributeValueIfRandomAbove(25),
                Efficiency = GetAttributeValueIfRandomAbove(25),
                Range = GetAttributeValueIfRandomAbove(25),
                Accuracy = GetAttributeValueIfRandomAbove(25),
                Speed = GetAttributeValueIfRandomAbove(25),
                Recovery = GetAttributeValueIfRandomAbove(25),
                Duration = GetAttributeValueIfRandomAbove(25)
            };
        }

        private string GetRandomEffect()
        {
            return Spell.LootEffectsAndOptions.ElementAt(_random.Next(0, Spell.LootEffectsAndOptions.Count - 1));
        }

        private int GetBiasedNumber(int min, int max)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(_random.NextDouble(), 3), 0);
        }

        internal ItemBase GetLootDrop()
        {
            //todo: limit good drops to higher level players
            //todo: add small posibility of returning a Relic

            var lootDrop = new ItemBase
            {
                Id = Guid.NewGuid().ToString(),
                Attributes = GetRandomAttributes(),
                Effects = new List<string>()
            };

            var isMagical = _random.Next(0, 2) > 0;
            if (isMagical)
            {
                lootDrop.Name = ItemBase.LootPrefixShard;
                //todo: icon

                var numberOfEffects = GetBiasedNumber(1, 4);
                for (var i = 1; i <= numberOfEffects; i++)
                {
                    string effect;
                    do { effect = GetRandomEffect(); }
                    while (lootDrop.Effects.Contains(effect));
                    lootDrop.Effects.Add(effect);
                }

                var elementalEffect = lootDrop.Effects.Intersect(Spell.ElementalEffects.All).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(elementalEffect) && Spell.LingeringPairing.ContainsKey(elementalEffect) && _random.Next(0, 2) > 0)
                {
                    lootDrop.Effects.Add(Spell.LingeringPairing[elementalEffect]);
                }

                //Debug.Log($"Added {numberOfEffects} effects: {string.Join(", ", lootDrop.Effects)}");
            }
            else
            {
                lootDrop.Name = ItemBase.LootPrefixScrap;
                //todo: icon
            }

            return lootDrop;
        }

        //todo: add ability to name item
        //todo: add validation e.g. enough scrap to make a two-handed weapon
        //todo: add validation e.g. at least one effect for a spell
        //todo: add a min level to craftedResult

        internal Spell GetSpell(IEnumerable<ItemBase> components)
        {
            var effects = components.SelectMany(x => x.Effects);
            var spell = new Spell
            {
                Id = Guid.NewGuid().ToString(),
                Targeting = GetTargeting(effects),
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery),
                    Duration = ComputeAttribute(components, x => x.Attributes.Duration)
                },
                Effects = GetEffects(CraftingTypeSpell, effects),
            };
            spell.Shape = GetShape(spell.Targeting, effects);

            if (spell.Effects.Count > 0)
            {
                spell.Name = spell.Effects.First() + " Spell";
            }
            else
            {
                spell.Name = spell.Targeting + " Spell";
            }

            return spell;
        }

        private string GetItemName(string prefix, GearBase item, string suffix)
        {
            return $"{prefix} {item.Attributes.Strength} {suffix}";
        }

        internal Weapon FillMeleeWeapon(Weapon weapon, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            weapon.Id = Guid.NewGuid().ToString();
            weapon.IsTwoHanded = weapon.EnforceTwoHanded || (weapon.AllowTwoHanded && isTwoHanded);
            weapon.Attributes = new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                Speed = ComputeAttribute(components, x => x.Attributes.Speed)
            };
            weapon.Effects = GetEffects(CraftingTypeWeapon, components.SelectMany(x => x.Effects));
            weapon.Name = GetItemName("Strength", weapon, weapon.TypeName);
            return weapon;
        }

        internal Weapon FillRangedWeapon(Weapon weapon, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            weapon.Id = Guid.NewGuid().ToString();
            weapon.IsTwoHanded = weapon.EnforceTwoHanded || (weapon.AllowTwoHanded && isTwoHanded);
            weapon.Attributes = new Attributes
            {
                IsAutomatic = weapon.AllowAutomatic && components.Any(x => x.Attributes.IsAutomatic),
                ExtraAmmoPerShot = components.FirstOrDefault(x => x.Attributes.ExtraAmmoPerShot > 0)?.Attributes.ExtraAmmoPerShot ?? 0,
                Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                Range = ComputeAttribute(components, x => x.Attributes.Range),
                Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
            };
            weapon.Effects = GetEffects(CraftingTypeWeapon, components.SelectMany(x => x.Effects));
            weapon.Name = GetItemName("Strength", weapon, weapon.TypeName);
            return weapon;
        }

        internal Weapon FillDefensiveWeapon(Weapon weapon, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            weapon.Id = Guid.NewGuid().ToString();
            weapon.IsTwoHanded = weapon.EnforceTwoHanded || (weapon.AllowTwoHanded && isTwoHanded);
            weapon.Attributes = new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
            };
            weapon.Effects = GetEffects(CraftingTypeWeapon, components.SelectMany(x => x.Effects));
            weapon.Name = GetItemName("Defence", weapon, weapon.TypeName);
            return weapon;
        }

        internal Armor FillArmor(Armor armor, IEnumerable<ItemBase> components)
        {
            armor.Id = Guid.NewGuid().ToString();
            armor.Attributes = new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Attributes.Strength)
            };
            armor.Effects = GetEffects(CraftingTypeArmor, components.SelectMany(x => x.Effects));
            armor.Name = GetItemName("Defence", armor, armor.TypeName);
            return armor;
        }

        internal Armor FillBarrier(Armor armor, IEnumerable<ItemBase> components)
        {
            armor.Id = Guid.NewGuid().ToString();
            armor.Attributes = new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
            };
            armor.Effects = GetEffects(CraftingTypeArmor, components.SelectMany(x => x.Effects));
            armor.Name = GetItemName("Defence", armor, armor.TypeName);
            return armor;
        }

        internal Accessory FillAccessory(Accessory accessory, IEnumerable<ItemBase> components)
        {
            accessory.Id = Guid.NewGuid().ToString();
            accessory.Attributes = new Attributes
            {
                Strength = ComputeAttribute(components, x => x.Attributes.Strength)
            };
            accessory.Effects = GetEffects(CraftingTypeAccessory, components.SelectMany(x => x.Effects));
            accessory.Name = GetItemName("Strength", accessory, accessory.TypeName);
            return accessory;
        }

        public ItemBase GetCraftedItem(string categoryName, string typeName, List<ItemBase> components, bool isTwoHanded)
        {
            var craftingResult = CraftingRegister.Instance.GetCraftingItem(categoryName, typeName);

            switch (craftingResult.Category)
            {
                case ICraftable.CraftingCategory.Spell:
                    return GetSpell(components);

                case ICraftable.CraftingCategory.Weapon:
                    var newWeapon = craftingResult as Weapon;
                    switch (newWeapon.SubCategory)
                    {
                        case ICraftableWeapon.WeaponCategory.Melee: return FillMeleeWeapon(newWeapon, components, isTwoHanded);
                        case ICraftableWeapon.WeaponCategory.Ranged: return FillRangedWeapon(newWeapon, components, isTwoHanded);
                        case ICraftableWeapon.WeaponCategory.Defensive: return FillDefensiveWeapon(newWeapon, components, isTwoHanded);
                        default: throw new Exception($"Unexpected weapon category '{newWeapon.SubCategory}'");
                    }

                case ICraftable.CraftingCategory.Armor:
                    var armor = craftingResult as Armor;
                    if (armor.SubCategory == ICraftableArmor.ArmorCategory.Barrier)
                    {
                        return FillBarrier(armor, components);
                    }
                    return FillArmor(armor, components);

                case ICraftable.CraftingCategory.Accessory:
                    var accessory = craftingResult as Accessory;
                    return FillAccessory(accessory, components);

                default:
                    throw new Exception($"Unexpected craftable category '{craftingResult.Category}'");
            }
        }

        public static string GetItemDescription(ItemBase item, bool includeName = true)
        {
            if (item == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            if (includeName) { sb.Append($"Name: {item.Name}\n"); }
            if (item.Attributes.IsAutomatic) { sb.Append("Automatic\n"); }
            if (item.Attributes.IsSoulbound) { sb.Append("Soulbound\n"); }
            if (item.Attributes.ExtraAmmoPerShot > 0) { sb.Append($"ExtraAmmoPerShot: {item.Attributes.ExtraAmmoPerShot}\n"); }
            if (item.Attributes.Strength > 0) { sb.Append($"Strength: {item.Attributes.Strength}\n"); }
            if (item.Attributes.Efficiency > 0) { sb.Append($"Efficiency: {item.Attributes.Efficiency}\n"); }
            if (item.Attributes.Range > 0) { sb.Append($"Range: {item.Attributes.Range}\n"); }
            if (item.Attributes.Accuracy > 0) { sb.Append($"Accuracy: {item.Attributes.Accuracy}\n"); }
            if (item.Attributes.Speed > 0) { sb.Append($"Speed: {item.Attributes.Speed}\n"); }
            if (item.Attributes.Recovery > 0) { sb.Append($"Recovery: {item.Attributes.Recovery}\n"); }
            if (item.Attributes.Duration > 0) { sb.Append($"Duration: {item.Attributes.Duration}\n"); }
            if (item.Effects.Count > 0) { sb.Append($"Effects: {string.Join(", ", item.Effects)}\n"); }

            return sb.ToString();
        }

    }
}
