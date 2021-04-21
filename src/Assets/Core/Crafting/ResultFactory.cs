using Assets.ApiScripts.Crafting;
using Assets.Core.Crafting.Base;
using Assets.Core.Crafting.SpellShapes;
using Assets.Core.Crafting.SpellTargeting;
using Assets.Core.Crafting.Types;
using Assets.Core.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

namespace Assets.Core.Crafting
{
    public class ResultFactory
    {
        public const string CraftingCategoryIdWeapon = "crafting.category.weapon";
        public const string CraftingCategoryIdArmor = "crafting.category.armor";
        public const string CraftingCategoryIdAccessory = "crafting.category.accessory";
        public const string CraftingCategoryIdSpell = "crafting.category.spell";

        private const string _attackNamePrefixId = "crafting.name.prefix.attack";
        private const string _defenceNamePrefixId = "crafting.name.prefix.defence";

        private static readonly Random _random = new Random();

        private readonly List<ILoot> _lootTypes;
        private readonly List<IEffect> _effectsForLoot;
        private readonly List<ISpellTargeting> _spellTargetingOptions;
        private readonly List<ISpellShape> _spellShapeOptions;

        public ResultFactory()
        {
            _lootTypes = ApiRegister.Instance
                .GetRegisteredTypes<ILoot>()
                .ToList();

            _effectsForLoot = ApiRegister.Instance.GetLootPossibilities();

            //todo: move these to a Core registry
            _spellTargetingOptions = new List<ISpellTargeting>
            {
                new Beam(),
                new Cone(),
                new Projectile(),
                new Self(),
                new Touch()
            };

            //todo: move these to a Core registry
            _spellShapeOptions = new List<ISpellShape>
            {
                new Wall(),
                new Zone()
            };
        }

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

        public ISpellTargeting GetSpellTargeting(string typeName)
        {
            return _spellTargetingOptions.First(x => x.TypeName == typeName);
        }

        private ISpellTargeting GetSpellTargeting(IEnumerable<IMagical> spellComponents)
        {
            //Exactly one targeting option
            var targeting = spellComponents.Select(x => x.Targeting.TypeName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (string.IsNullOrWhiteSpace(targeting))
            {
                targeting = nameof(Projectile);
            }

            return _spellTargetingOptions.First(x => x.TypeName == targeting);
        }

        public ISpellShape GetSpellShape(string typeName)
        {
            return _spellShapeOptions.First(x => x.TypeName == typeName);
        }

        private ISpellShape GetSpellShape(ISpellTargeting targeting, IEnumerable<IMagical> spellComponents)
        {
            //Only one shape, if any
            if (!targeting.HasShape)
            {
                return null;
            }

            var shape = spellComponents.Select(x => x.Shape.TypeName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            return _spellShapeOptions.FirstOrDefault(x => x.TypeName == shape);
        }

        private List<IEffect> GetEffects(string craftingType, IEnumerable<ItemBase> components)
        {
            var effects = components.Where(x => x.Effects != null).SelectMany(x => x.Effects);

            var elementalEffects = effects.Where(x => x is IElement);
            var elementalEffect = elementalEffects.FirstOrDefault();
            if (elementalEffect != null)
            {
                effects = effects
                    .Except(elementalEffects.Where(x => x != elementalEffect));
            }

            //todo: deal with lingering
            //var lingeringEffect = Spell.LingeringPairing.FirstOrDefault(x => x.Key == elementalEffect).Value;
            //effects = effects.Except(Spell.LingeringOptions.All.Where(x => x != lingeringEffect));

            if (craftingType == nameof(Armor) || craftingType == nameof(Accessory))
            {
                return effects
                    .Where(x =>
                         x is IEffectBuff
                        || x is IEffectSupport
                        || x is IElement)
                    .ToList();
            }

            if (craftingType == nameof(Weapon))
            {
                //todo: and lingering
                return effects
                    .Where(x =>
                        x is IEffectDebuff
                        || x is IElement)
                    .ToList();
            }

            if (craftingType != nameof(Spell))
            {
                throw new Exception($"Unexpected craftingType '{craftingType}'");
            }

            if (effects.Any(x => x is IEffectBuff || x is IEffectSupport))
            {
                effects = effects
                    .Where(x =>
                        !(x is IEffectDebuff)
                        && !(x is IElement));
            }

            return effects.ToList();
        }

        private bool IsLucky(int percentageChance)
        {
            return _random.Next(1, 101) <= percentageChance;
        }

        private int GetAttributeValueIfLucky(int percentageChance)
        {
            return IsLucky(percentageChance) ? _random.Next(1, 100) : 0;
        }

        private IEffect GetRandomEffect()
        {
            return _effectsForLoot.ElementAt(_random.Next(0, _effectsForLoot.Count));
        }

        private ISpellTargeting GetRandomSpellTargeting()
        {
            if (IsLucky(50))
            {
                return _spellTargetingOptions.ElementAt(_random.Next(0, _spellTargetingOptions.Count));
            }
            return null;
        }

        private ISpellShape GetRandomSpellShape()
        {
            if (IsLucky(10))
            {
                return _spellShapeOptions.ElementAt(_random.Next(0, _spellShapeOptions.Count));
            }
            return null;
        }

        private int GetBiasedNumber(int min, int max)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(_random.NextDouble(), 3), 0);
        }

        internal ItemBase GetLootDrop()
        {
            //todo: limit good drops to higher level players
            //todo: add small posibility of returning a Relic

            var lootDrop = new Loot
            {
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsAutomatic = IsLucky(50),
                    IsSoulbound = IsLucky(10),
                    ExtraAmmoPerShot = IsLucky(20) ? _random.Next(1, 4) : 0,
                    Strength = GetAttributeValueIfLucky(75),
                    Efficiency = GetAttributeValueIfLucky(75),
                    Range = GetAttributeValueIfLucky(75),
                    Accuracy = GetAttributeValueIfLucky(75),
                    Speed = GetAttributeValueIfLucky(75),
                    Recovery = GetAttributeValueIfLucky(75),
                    Duration = GetAttributeValueIfLucky(75)
                }
            };

            var magicalLootTypes = _lootTypes.Where(x => x.Category == ILoot.LootCategory.Magic);
            var techLootTypes = _lootTypes.Where(x => x.Category == ILoot.LootCategory.Technology);

            var isMagical = magicalLootTypes.Any() && IsLucky(50);
            if (isMagical)
            {
                lootDrop.RegistryType = magicalLootTypes
                    .OrderBy(x => _random.Next())
                    .First();

                var effects = new List<IEffect>();
                var numberOfEffects = GetBiasedNumber(1, Math.Min(4, _effectsForLoot.Count));
                for (var i = 1; i <= numberOfEffects; i++)
                {
                    IEffect effect;
                    var debugCounter = 0;
                    do
                    {
                        effect = GetRandomEffect();
                        debugCounter++;
                    }
                    while (debugCounter < 10 && (effect == null || effects.Contains(effect)));
                    effects.Add(effect);

                    if (debugCounter >= 10)
                    {
                        UnityEngine.Debug.LogError("Infinite loop situation here. Go fix it!");
                    }
                }

                //todo: deal with lingering
                //var elementalEffect = lootDrop.Effects.Intersect(Spell.ElementalEffects.All).FirstOrDefault();
                //if (!string.IsNullOrWhiteSpace(elementalEffect) && Spell.LingeringPairing.ContainsKey(elementalEffect) && ChanceWin(50) == 0)
                //{
                //    lootDrop.Effects.Add(Spell.LingeringPairing[elementalEffect]);
                //}

                lootDrop.Effects = effects.ToList();

                lootDrop.Targeting = GetRandomSpellTargeting();
                lootDrop.Shape = GetRandomSpellShape();

                //Debug.Log($"Added {numberOfEffects} effects: {string.Join(", ", lootDrop.Effects)}");
            }
            else
            {
                lootDrop.RegistryType = techLootTypes
                    .OrderBy(x => _random.Next())
                    .First();
            }

            lootDrop.Name = Localizer.Instance.GetTranslatedTypeName(lootDrop.RegistryType);

            //todo: icon

            return lootDrop;
        }

        //todo: add ability to name item
        //todo: add validation e.g. enough scrap to make a two-handed weapon
        //todo: add validation e.g. at least one effect for a spell
        //todo: add a min level to craftedResult

        private Spell GetSpell(IEnumerable<ItemBase> components)
        {
            var spellComponents = components.OfType<IMagical>();

            var targeting = GetSpellTargeting(spellComponents);

            var spell = new Spell
            {
                Id = Guid.NewGuid().ToString(),
                Targeting = targeting,
                Shape = GetSpellShape(targeting, spellComponents),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery),
                    Duration = ComputeAttribute(components, x => x.Attributes.Duration)
                },
                Effects = GetEffects(nameof(Spell), components)
            };

            var suffix = Localizer.Instance.Translate(ResultFactory.CraftingCategoryIdSpell);

            if (spell.Effects.Count > 0)
            {
                //todo: localise effect
                spell.Name = spell.Effects.First().TypeName + " " + suffix;
            }
            else
            {
                //todo: localise targeting 
                spell.Name = $"{Localizer.Instance.Translate(_attackNamePrefixId)} {spell.Attributes.Strength} {spell.Targeting} {suffix}";
            }

            return spell;
        }

        private string GetItemName(string prefixTranslationId, GearBase item)
        {
            return $"{Localizer.Instance.Translate(prefixTranslationId)} {item.Attributes.Strength} {Localizer.Instance.GetTranslatedTypeName(item.RegistryType)}";
        }

        private Weapon GetMeleeWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed)
                },
                Effects = GetEffects(nameof(Weapon), components)
            };
            weapon.Name = GetItemName(_attackNamePrefixId, weapon);
            return weapon;
        }

        private Weapon GetRangedWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    IsAutomatic = craftableType.AllowAutomatic && components.Any(x => x.Attributes.IsAutomatic),
                    ExtraAmmoPerShot = components.FirstOrDefault(x => x.Attributes.ExtraAmmoPerShot > 0)?.Attributes.ExtraAmmoPerShot ?? 0,
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(nameof(Weapon), components)
            };
            weapon.Name = GetItemName(_attackNamePrefixId, weapon);
            return weapon;
        }

        private Weapon GetDefensiveWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(nameof(Weapon), components)
            };
            weapon.Name = GetItemName(_defenceNamePrefixId, weapon);
            return weapon;
        }

        private Armor GetArmor(IGearArmor craftableType, IEnumerable<ItemBase> components)
        {
            var armor = new Armor()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(nameof(Armor), components)
            };
            armor.Name = GetItemName(_defenceNamePrefixId, armor);
            return armor;
        }

        private Armor GetBarrier(IGearArmor craftableType, IEnumerable<ItemBase> components)
        {
            var armor = new Armor()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(nameof(Armor), components)
            };
            armor.Name = GetItemName(_defenceNamePrefixId, armor);
            return armor;
        }

        private Accessory GetAccessory(IGearAccessory craftableType, IEnumerable<ItemBase> components)
        {
            var accessory = new Accessory()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(nameof(Accessory), components)
            };
            accessory.Name = GetItemName(_attackNamePrefixId, accessory);
            return accessory;
        }

        public ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IEnumerable<ItemBase> components)
        {
            if (categoryName == nameof(Spell))
            {
                return GetSpell(components);
            }

            switch (categoryName)
            {
                case nameof(Weapon):
                    var craftableWeapon = ApiRegister.Instance.GetRegisteredByTypeName<IGearWeapon>(typeName);
                    switch (craftableWeapon.Category)
                    {
                        case IGearWeapon.WeaponCategory.Melee: return GetMeleeWeapon(craftableWeapon, components, isTwoHanded);
                        case IGearWeapon.WeaponCategory.Ranged: return GetRangedWeapon(craftableWeapon, components, isTwoHanded);
                        case IGearWeapon.WeaponCategory.Defensive: return GetDefensiveWeapon(craftableWeapon, components, isTwoHanded);
                        default: throw new Exception($"Unexpected weapon category '{craftableWeapon.Category}'");
                    }

                case nameof(Armor):
                    var craftableArmor = ApiRegister.Instance.GetRegisteredByTypeName<IGearArmor>(typeName);
                    if (craftableArmor.InventorySlot == IGearArmor.ArmorSlot.Barrier)
                    {
                        return GetBarrier(craftableArmor, components);
                    }
                    return GetArmor(craftableArmor, components);

                case nameof(Accessory):
                    var craftableAccessory = ApiRegister.Instance.GetRegisteredByTypeName<IGearAccessory>(typeName);
                    return GetAccessory(craftableAccessory, components);

                default:
                    throw new Exception($"Unexpected craftable category '{categoryName}'");
            }
        }

        public static string GetItemDescription(ItemBase item, bool includeName = true)
        {
            if (item == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            if (includeName) { sb.Append($"{Localizer.Instance.Translate("attributes.name")}: {item.Name}\n"); }
            if (item.Attributes.IsAutomatic) { sb.Append(Localizer.Instance.Translate("attributes.isautomatic") + "\n"); }
            if (item.Attributes.IsSoulbound) { sb.Append(Localizer.Instance.Translate("attributes.issoulbound") + "\n"); }
            if (item.Attributes.ExtraAmmoPerShot > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.extraammopershot")}: {item.Attributes.ExtraAmmoPerShot}\n"); }
            if (item.Attributes.Strength > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.strength")}: {item.Attributes.Strength}\n"); }
            if (item.Attributes.Efficiency > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.efficiency")}: {item.Attributes.Efficiency}\n"); }
            if (item.Attributes.Range > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.range")}: {item.Attributes.Range}\n"); }
            if (item.Attributes.Accuracy > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.accuracy")}: {item.Attributes.Accuracy}\n"); }
            if (item.Attributes.Speed > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.speed")}: {item.Attributes.Speed}\n"); }
            if (item.Attributes.Recovery > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.recovery")}: {item.Attributes.Recovery}\n"); }
            if (item.Attributes.Duration > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.duration")}: {item.Attributes.Duration}\n"); }
            if (item.Effects != null && item.Effects.Count > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.effects")}: {string.Join(", ", item.Effects.Select(x => x.TypeName))}\n"); }
            if (item.Attributes.Duration > 0) { sb.Append($"{Localizer.Instance.Translate("attributes.duration")}: {item.Attributes.Duration}\n"); }

            if (item is IMagical spell)
            {
                //todo: localise
                if (spell.Targeting != null) { sb.Append($"{Localizer.Instance.Translate("attributes.spelltargeting")}: {spell.Targeting.TypeName}\n"); }
                if (spell.Shape != null) { sb.Append($"{Localizer.Instance.Translate("attributes.spellshape")}: {spell.Shape.TypeName}\n"); }
            }

            return sb.ToString();
        }

    }
}
