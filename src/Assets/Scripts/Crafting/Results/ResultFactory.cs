using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace Assets.Scripts.Crafting.Results
{
    public class ResultFactory
    {
        public const string LootPrefixScrap = "Scrap";
        public const string LootPrefixShard = "Shard";

        // ReSharper disable once InconsistentNaming
        private static readonly Random _random = new Random();

        private int ComputeAttribute(IEnumerable<ItemBase> components, Func<ItemBase, int> getProp, bool allowMax = true)
        {
            var withValue = components.Where(x => getProp(x) > 0);

            if (!withValue.Any())
            {
                return 0;
            }

            var min = withValue.Min(getProp);
            var max = withValue.Max(getProp);
            var topEndSkew = max - ((max - min) / 10);

            int result = allowMax
                ? topEndSkew
                : (int)Math.Round(topEndSkew - (0.009 * (topEndSkew - 50)), MidpointRounding.AwayFromZero);

            //Debug.Log($"{getProp.Method.Name} = Min:{min}, Max:{max}, Skew:{topEndSkew}, Result:{result}");

            return result;
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
            //Cannot cast "tap" buffs
            var effects = effectsInput
                .Except(new[] { Spell.BuffEffects.LifeTap, Spell.BuffEffects.ManaTap })
                .Except(Spell.TargetingOptions.All)
                .Except(Spell.ShapeOptions.All);

            //Only one elemental effect
            var elementalEffects = effects.Intersect(Spell.ElementalEffects.All);
            var elementalEffect = elementalEffects.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(elementalEffect))
            {
                effects = effects
                    .Except(Spell.ElementalEffects.All.Where(x => x != elementalEffect));
            }

            //Lingering must have matching elemental type
            var lingeringEffect = Spell.LingeringPairing.FirstOrDefault(x => x.Key == elementalEffect).Value;
            effects = effects.Except(Spell.LingeringOptions.All.Where(x => x != lingeringEffect));

            if (craftingType == CraftingUi.CraftingTypeArmor || craftingType == CraftingUi.CraftingTypeAccessory)
            {
                return effects.Intersect(Spell.BuffEffects.All)
                    .Union(effects.Intersect(Spell.SupportEffects.All))
                    .Union(effects.Intersect(Spell.ElementalEffects.All))
                    .ToList();
            }

            //Weapons have debuffs and elemental (+lingering) only
            if (craftingType == CraftingUi.CraftingTypeWeapon)
            {
                return effects.Intersect(Spell.DebuffEffects.All)
                    .Union(effects.Intersect(Spell.ElementalEffects.All))
                    .Union(effects.Intersect(Spell.LingeringOptions.All))
                    .ToList();
            }

            if (craftingType != CraftingUi.CraftingTypeSpell)
            {
                throw new Exception($"Unexpected craftingType '{craftingType}'");
            }

            //If there is a buff or support then remove all debuffs and "offensive" effects
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
                lootDrop.Name = LootPrefixShard;
                //todo: icon

                var numberOfEffects = GetBiasedNumber(1, 4);
                for (var i = 1; i <= numberOfEffects; i++)
                {
                    string effect;
                    do { effect = GetRandomEffect(); }
                    while (lootDrop.Effects.Contains(effect));
                    lootDrop.Effects.Add(effect);
                }

                //If lingering is appropriate, add a chance of it being included
                var elementalEffect = lootDrop.Effects.Intersect(Spell.ElementalEffects.All).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(elementalEffect) && Spell.LingeringPairing.ContainsKey(elementalEffect) && _random.Next(0, 2) > 0)
                {
                    lootDrop.Effects.Add(Spell.LingeringPairing[elementalEffect]);
                }

                //Debug.Log($"Added {numberOfEffects} effects: {string.Join(", ", lootDrop.Effects)}");
            }
            else
            {
                lootDrop.Name = LootPrefixScrap;
                //todo: icon
            }

            return lootDrop;
        }

        //todo: add ability to break down an item/spell
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
                Effects = GetEffects(CraftingUi.CraftingTypeSpell, effects),
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

        private string GetItemName(string prefix, GearBase item)
        {
            return $"{prefix} {item.Attributes.Strength} {item.Type}";
        }

        internal Weapon GetMeleeWeapon(string type, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var item = new Weapon
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                IsTwoHanded = isTwoHanded,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed)
                },
                Effects = GetEffects(CraftingUi.CraftingTypeWeapon, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Strength", item);
            return item;
        }

        internal Weapon GetRangedWeapon(string type, IEnumerable<ItemBase> components, bool isTwoHanded, bool allowAutomatic)
        {
            var item = new Weapon
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                IsTwoHanded = isTwoHanded,
                Attributes = new Attributes
                {
                    IsAutomatic = allowAutomatic && components.Any(x => x.Attributes.IsAutomatic),
                    ExtraAmmoPerShot = components.FirstOrDefault(x => x.Attributes.ExtraAmmoPerShot > 0)?.Attributes.ExtraAmmoPerShot ?? 0,
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(CraftingUi.CraftingTypeWeapon, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Strength", item);
            return item;
        }

        internal Weapon GetShield(IEnumerable<ItemBase> components)
        {
            var item = new Weapon
            {
                Id = Guid.NewGuid().ToString(),
                Type = Weapon.Shield,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(CraftingUi.CraftingTypeWeapon, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Defence", item);
            return item;
        }

        internal Armor GetArmor(string type, IEnumerable<ItemBase> components)
        {
            var item = new Armor
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Unnamed Armor",
                Type = type,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(CraftingUi.CraftingTypeArmor, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Defence", item);
            return item;
        }

        internal Armor GetBarrier(IEnumerable<ItemBase> components)
        {
            var item = new Armor
            {
                Id = Guid.NewGuid().ToString(),
                Type = Armor.Barrier,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(CraftingUi.CraftingTypeArmor, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Defence", item);
            return item;
        }

        internal Accessory GetAccessory(string type, IEnumerable<ItemBase> components)
        {
            var item = new Accessory
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(CraftingUi.CraftingTypeAccessory, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Strength", item);
            return item;
        }

        public ItemBase GetCraftedItem(List<ItemBase> components, string selectedType, string selectedSubtype, bool isTwoHanded)
        {
            //todo: requirements e.g. strength, speed, accuracy, 6 scrap or less

            if (selectedType == CraftingUi.CraftingTypeSpell)
            {
                return GetSpell(components);
            }
            else
            {
                switch (selectedSubtype)
                {
                    case Weapon.Dagger: return GetMeleeWeapon(Weapon.Dagger, components, false);
                    case Weapon.Spear: return GetMeleeWeapon(Weapon.Spear, components, true);
                    case Weapon.Bow: return GetRangedWeapon(Weapon.Bow, components, true, false);
                    case Weapon.Crossbow: return GetRangedWeapon(Weapon.Crossbow, components, true, false);
                    case Weapon.Shield: return GetShield(components);

                    case Armor.Helm: return GetArmor(Armor.Helm, components);
                    case Armor.Chest: return GetArmor(Armor.Chest, components);
                    case Armor.Legs: return GetArmor(Armor.Legs, components);
                    case Armor.Feet: return GetArmor(Armor.Feet, components);
                    case Armor.Gloves: return GetArmor(Armor.Gloves, components);
                    case Armor.Barrier: return GetBarrier(components);

                    case Accessory.Amulet: return GetAccessory(Accessory.Amulet, components);
                    case Accessory.Ring: return GetAccessory(Accessory.Ring, components);
                    case Accessory.Belt: return GetAccessory(Accessory.Belt, components);

                    default:

                        switch (selectedSubtype)
                        {
                            case Weapon.Axe: return GetMeleeWeapon(Weapon.Axe, components, isTwoHanded);
                            case Weapon.Sword: return GetMeleeWeapon(Weapon.Sword, components, isTwoHanded);
                            case Weapon.Hammer: return GetMeleeWeapon(Weapon.Hammer, components, isTwoHanded);
                            case Weapon.Gun: return GetRangedWeapon(Weapon.Gun, components, isTwoHanded, true);
                            default:
                                throw new System.Exception("Invalid weapon type");
                        }
                }
            }
        }

    }
}
