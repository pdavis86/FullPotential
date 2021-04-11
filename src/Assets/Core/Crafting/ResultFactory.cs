using Assets.ApiScripts.Crafting;
using Assets.Core.Crafting.SpellShapes;
using Assets.Core.Crafting.SpellTargeting;
using Assets.Core.Crafting.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable PossibleMultipleEnumeration

namespace Assets.Core.Crafting
{
    public class ResultFactory
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Random _random = new Random();

        private readonly List<IGearLoot> _lootTypes;
        private readonly List<IEffect> _effectsForLoot;
        private readonly List<ISpellTargeting> _spellTargetingOptions;
        private readonly List<ISpellShape> _spellShapeOptions;
        private readonly List<string> _spellOptionNames;

        public ResultFactory()
        {
            _lootTypes = ApiRegister.Instance
                .GetCraftables<IGearLoot>()
                .Select(x => x as IGearLoot)
                .ToList();

            _effectsForLoot = ApiRegister.Instance.GetLootPossibilities();

            _spellTargetingOptions = new List<ISpellTargeting>
            {
                new SpellTargeting.Beam(),
                new SpellTargeting.Cone(),
                new SpellTargeting.Projectile(),
                new SpellTargeting.Self(),
                new SpellTargeting.Touch()
            };

            _spellShapeOptions = new List<ISpellShape>
            {
                new SpellShapes.Wall(),
                new SpellShapes.Zone()
            };

            _spellOptionNames = new List<string>();
            _spellOptionNames.AddRange(_spellTargetingOptions.Select(x => x.TypeName));
            _spellOptionNames.AddRange(_spellShapeOptions.Select(x => x.TypeName));
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

        private ISpellTargeting GetTargeting(IEnumerable<IMagical> spellComponents)
        {
            //Exactly one targeting option
            var targeting = spellComponents.Select(x => x.GetTargetingTypeName()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (string.IsNullOrWhiteSpace(targeting))
            {
                targeting = nameof(SpellTargeting.Projectile);
            }

            return _spellTargetingOptions.First(x => x.TypeName == targeting);
        }

        private ISpellShape GetShape(ISpellTargeting targeting, IEnumerable<IMagical> spellComponents)
        {
            //Only one shape, if any
            if (!targeting.HasShape)
            {
                return null;
            }

            var shape = spellComponents.Select(x => x.GetShapeTypeName()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
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

        private IEffect GetRandomEffect()
        {
            return _effectsForLoot.ElementAt(_random.Next(0, _effectsForLoot.Count));
        }

        private ISpellTargeting GetRandomSpellTargeting()
        {
            if (_random.Next(0, 2) > 0)
            {
                return null;
            }
            return _spellTargetingOptions.ElementAt(_random.Next(0, _spellTargetingOptions.Count));
        }

        private ISpellShape GetRandomSpellShape()
        {
            if (_random.Next(0, 2) > 0)
            {
                return null;
            }
            return _spellShapeOptions.ElementAt(_random.Next(0, _spellShapeOptions.Count));
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
                Attributes = GetRandomAttributes()
            };

            var isMagical = _random.Next(0, 2) > 0;
            if (isMagical)
            {
                lootDrop.CraftableType = _lootTypes
                    .Where(x => x.Category == IGearLoot.LootCategory.Magic)
                    .OrderBy(x => _random.Next())
                    .FirstOrDefault();

                var effects = new List<IEffect>();
                var numberOfEffects = GetBiasedNumber(1, Math.Min(4, _effectsForLoot.Count));
                var debugCounter = 0;
                for (var i = 1; i <= numberOfEffects; i++)
                {
                    IEffect effect;
                    debugCounter = 0;
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
                //if (!string.IsNullOrWhiteSpace(elementalEffect) && Spell.LingeringPairing.ContainsKey(elementalEffect) && _random.Next(0, 2) > 0)
                //{
                //    lootDrop.Effects.Add(Spell.LingeringPairing[elementalEffect]);
                //}

                lootDrop.Effects = effects.ToList();

                lootDrop.Targeting = GetRandomSpellTargeting()?.TypeName;
                lootDrop.Shape = GetRandomSpellShape()?.TypeName;

                //Debug.Log($"Added {numberOfEffects} effects: {string.Join(", ", lootDrop.Effects)}");
            }
            else
            {
                lootDrop.CraftableType = _lootTypes
                    .Where(x => x.Category == IGearLoot.LootCategory.Technology)
                    .OrderBy(x => _random.Next())
                    .FirstOrDefault();
            }

            lootDrop.Name = lootDrop.CraftableType.TypeName;

            //todo: icon

            return lootDrop;
        }

        //todo: add ability to name item
        //todo: add validation e.g. enough scrap to make a two-handed weapon
        //todo: add validation e.g. at least one effect for a spell
        //todo: add a min level to craftedResult

        internal Spell GetSpell(IEnumerable<ItemBase> components)
        {
            var spellComponents = components.Where(x => x is IMagical).Select(x => x as IMagical);

            var targeting = GetTargeting(spellComponents);

            var spell = new Spell
            {
                Id = Guid.NewGuid().ToString(),
                Targeting = targeting.TypeName,
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
            spell.Shape = GetShape(targeting, spellComponents)?.TypeName;

            if (spell.Effects.Count > 0)
            {
                spell.Name = spell.Effects.First().TypeName + " Spell";
            }
            else
            {
                spell.Name = $"Strength {spell.Attributes.Strength} {spell.Targeting} Spell";
            }

            return spell;
        }

        private string GetItemName(string prefix, GearBase item, string suffix)
        {
            return $"{prefix} {item.Attributes.Strength} {suffix}";
        }

        internal Weapon GetMeleeWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                CraftableType = craftableType,
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
            weapon.Name = GetItemName("Strength", weapon, weapon.CraftableType.TypeName);
            return weapon;
        }

        internal Weapon GetRangedWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                CraftableType = craftableType,
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
            weapon.Name = GetItemName("Strength", weapon, weapon.CraftableType.TypeName);
            return weapon;
        }

        internal Weapon GetDefensiveWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                CraftableType = craftableType,
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
            weapon.Name = GetItemName("Defence", weapon, weapon.CraftableType.TypeName);
            return weapon;
        }

        internal Armor GetArmor(IGearArmor craftableType, IEnumerable<ItemBase> components)
        {
            var armor = new Armor()
            {
                CraftableType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(nameof(Armor), components)
            };
            armor.Name = GetItemName("Defence", armor, armor.CraftableType.TypeName);
            return armor;
        }

        internal Armor GetBarrier(IGearArmor craftableType, IEnumerable<ItemBase> components)
        {
            var armor = new Armor()
            {
                CraftableType = craftableType,
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
            armor.Name = GetItemName("Defence", armor, armor.CraftableType.TypeName);
            return armor;
        }

        internal Accessory GetAccessory(IGearAccessory craftableType, IEnumerable<ItemBase> components)
        {
            var accessory = new Accessory()
            {
                CraftableType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(nameof(Accessory), components)
            };
            accessory.Name = GetItemName("Strength", accessory, accessory.CraftableType.TypeName);
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
                    var craftableWeapon = ApiRegister.Instance.GetCraftableTypeByName<IGearWeapon>(typeName);
                    switch (craftableWeapon.Category)
                    {
                        case IGearWeapon.WeaponCategory.Melee: return GetMeleeWeapon(craftableWeapon, components, isTwoHanded);
                        case IGearWeapon.WeaponCategory.Ranged: return GetRangedWeapon(craftableWeapon, components, isTwoHanded);
                        case IGearWeapon.WeaponCategory.Defensive: return GetDefensiveWeapon(craftableWeapon, components, isTwoHanded);
                        default: throw new Exception($"Unexpected weapon category '{craftableWeapon.Category}'");
                    }

                case nameof(Armor):
                    var craftableArmor = ApiRegister.Instance.GetCraftableTypeByName<IGearArmor>(typeName);
                    if (craftableArmor.InventorySlot == IGearArmor.ArmorSlot.Barrier)
                    {
                        return GetBarrier(craftableArmor, components);
                    }
                    return GetArmor(craftableArmor, components);

                case nameof(Accessory):
                    var craftableAccessory = ApiRegister.Instance.GetCraftableTypeByName<IGearAccessory>(typeName);
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

            var effects = item.Effects;

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
            if (effects != null && effects.Count > 0) { sb.Append($"Effects: {string.Join(", ", effects.Select(x => x.TypeName))}\n"); }

            return sb.ToString();
        }

    }
}
