using Assets.Scripts.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace Assets.Scripts.Crafting.Results
{
    [ServerSideOnlyTemp]
    public class ResultFactory
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Random _random = new Random();

        private int GetValue(int rarityThreshold)
        {
            return _random.Next(0, 100) > rarityThreshold ? _random.Next(1, 100) : 0;
        }

        private int ComputeAttribute(List<ItemBase> components, Func<ItemBase, int> getProp)
        {
            var min = components.Min(getProp);
            var max = components.Max(getProp);

            var topEndSkew = max - ((max - min) / 10);

            int result;
            if (_random.Next(1, 11) < 9)
            {
                result = (int)Math.Round(topEndSkew - (0.009 * topEndSkew), MidpointRounding.AwayFromZero);
            }
            else
            {
                result = topEndSkew;
            }

            if (result == 0)
            {
                result = 1;
            }

            //Debug.Log($"{getProp.Method.Name} = Min:{min}, Max:{max}, Skew:{topEndSkew}, Result:{result}");

            return result;
        }

        private int PickValueAtRandom(List<ItemBase> components, Func<ItemBase, int> getProp)
        {
            var values = components.Select(getProp).ToList();
            var takeAt = _random.Next(0, values.Count - 1);
            return values.ElementAt(takeAt);
        }

        private string GetTargeting(IEnumerable<string> effectsInput)
        {
            //Only one target
            var target = effectsInput.Intersect(Spell.TargetingOptions.All).LastOrDefault();
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
                var shape = effectsInput.Intersect(Spell.ShapeOptions.All).LastOrDefault();
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

            //If there is a buff or support then remove all debuffs
            if (effects.Intersect(Spell.BuffEffects.All).Any() || effects.Intersect(Spell.SupportEffects.All).Any())
            {
                effects = effects.Except(Spell.DebuffEffects.All);
            }

            //Lingering must have matching elemental type
            var elementalEffects = effects.Intersect(Spell.ElementalEffects.All);
            var lingeringOptions = effects.Intersect(Spell.LingeringOptions.All);
            if (lingeringOptions.Any())
            {
                var elementalEffectFound = false;
                foreach (var option in lingeringOptions)
                {
                    var expectedElementalEffect = Spell.LingeringPairing.FirstOrDefault(x => x.Value == option).Key;
                    if (!string.IsNullOrWhiteSpace(expectedElementalEffect) && elementalEffects.Contains(expectedElementalEffect))
                    {
                        elementalEffectFound = true;
                        effects = effects.Except(Spell.LingeringOptions.All.Where(x => x != option));
                        effects = effects.Except(Spell.ElementalEffects.All.Where(x => x != expectedElementalEffect));
                        break;
                    }
                }
                if (!elementalEffectFound)
                {
                    effects = effects.Except(Spell.LingeringOptions.All);
                }

                elementalEffects = effects.Intersect(Spell.ElementalEffects.All);
            }

            //Remove all but the last elemental effect
            if (elementalEffects.Count() > 1)
            {
                var lastElementalEffect = elementalEffects.Last();
                effects = effects
                    .Except(Spell.ElementalEffects.All.Where(x => x != lastElementalEffect));
            }

            if (craftingType == ChooseCraftingType.CraftingTypeArmor || craftingType == ChooseCraftingType.CraftingTypeAccessory)
            {
                return effects.Intersect(Spell.BuffEffects.All)
                    .Union(effects.Intersect(Spell.SupportEffects.All))
                    .Union(effects.Intersect(Spell.ElementalEffects.All))
                    .ToList();
            }

            //Weapons have debuffs and elemental (+lingering) only
            if (craftingType == ChooseCraftingType.CraftingTypeWeapon)
            {
                return effects.Intersect(Spell.DebuffEffects.All)
                    .Union(effects.Intersect(Spell.ElementalEffects.All))
                    .Union(effects.Intersect(Spell.LingeringOptions.All))
                    .ToList();
            }

            if (craftingType != ChooseCraftingType.CraftingTypeSpell)
            {
                throw new Exception($"Unexpected craftingType '{craftingType}'");
            }

            return effects.ToList();
        }

        private Attributes GetRandomAttributes()
        {
            return new Attributes
            {
                IsActivated = _random.Next(0, 100) > 50,
                IsAutomatic = _random.Next(0, 100) > 50,
                IsSoulbound = _random.Next(0, 100) > 90,
                ExtraAmmoPerShot = _random.Next(0, 100) > 70 ? _random.Next(1, 2) : 0,
                Strength = GetValue(25),
                Cost = GetValue(25),
                Range = GetValue(25),
                Accuracy = GetValue(25),
                Speed = GetValue(25),
                Recovery = GetValue(25),
                Duration = GetValue(25)
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

            var lootDrop = new ItemBase
            {
                Attributes = GetRandomAttributes(),
                Effects = new List<string>()
            };

            var isMagical = _random.Next(0, 2) > 0;
            if (isMagical)
            {
                lootDrop.Name = "Shard";
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
                if (!string.IsNullOrWhiteSpace(elementalEffect) && _random.Next(0, 2) > 0)
                {
                    lootDrop.Effects.Add(Spell.LingeringPairing[elementalEffect]);
                }

                //Debug.Log($"Added {numberOfEffects} effects: {string.Join(", ", lootDrop.Effects)}");
            }
            else
            {
                lootDrop.Name = "Scrap";
                //todo: icon
            }

            return lootDrop;
        }

        //todo: add ability to break down an item/spell
        //todo: add ability to name item
        //todo: add validation e.g. enough scrap to make a two-handed weapon
        //todo: add validation e.g. at least one effect for a spell
        //todo: add a min level to craftedResult

        internal ItemBase GetSpell(List<ItemBase> components)
        {
            var effects = components.SelectMany(x => x.Effects);
            var spell = new Spell
            {
                Targeting = GetTargeting(effects),
                Attributes = new Attributes
                {
                    IsActivated = true,
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Cost = ComputeAttribute(components, x => x.Attributes.Cost),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery),
                    Duration = ComputeAttribute(components, x => x.Attributes.Duration)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeSpell, effects),
            };
            spell.Shape = GetShape(spell.Targeting, effects);
            spell.Name = spell.Effects.First() + " Spell";
            return spell;
        }

        private string GetItemName(string prefix, GearBase item)
        {
            return $"{prefix} {item.Attributes.Strength} {item.Type}";
        }

        internal ItemBase GetMeleeWeapon(string type, List<ItemBase> components, bool isTwoHanded)
        {
            var item = new Weapon
            {
                Type = type,
                IsTwoHanded = isTwoHanded,
                Attributes = new Attributes
                {
                    IsActivated = components.Any(x => x.Attributes.IsActivated),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeWeapon, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Strength", item);
            return item;
        }

        internal ItemBase GetRangedWeapon(string type, List<ItemBase> components, bool isTwoHanded)
        {
            var item = new Weapon
            {
                Type = type,
                IsTwoHanded = isTwoHanded,
                Attributes = new Attributes
                {
                    IsActivated = components.Any(x => x.Attributes.IsActivated),
                    IsAutomatic = components.Any(x => x.Attributes.IsAutomatic),
                    ExtraAmmoPerShot = PickValueAtRandom(components, x => x.Attributes.ExtraAmmoPerShot),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Cost = ComputeAttribute(components, x => x.Attributes.Cost),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeWeapon, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Strength", item);
            return item;
        }

        internal ItemBase GetShield(List<ItemBase> components)
        {
            var item = new Weapon
            {
                Type = Weapon.Shield,
                Attributes = new Attributes
                {
                    IsActivated = components.Any(x => x.Attributes.IsActivated),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeWeapon, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Defence", item);
            return item;
        }

        internal ItemBase GetArmor(string type, List<ItemBase> components)
        {
            var item = new Armor
            {
                Name = "Unnamed Armor",
                Type = type,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeArmor, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Defence", item);
            return item;
        }

        internal ItemBase GetBarrier(List<ItemBase> components)
        {
            var item = new Armor
            {
                Type = Armor.Barrier,
                Attributes = new Attributes
                {
                    IsActivated = components.Any(x => x.Attributes.IsActivated),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Cost = ComputeAttribute(components, x => x.Attributes.Cost),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeArmor, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Defence", item);
            return item;
        }

        internal ItemBase GetAccessory(string type, List<ItemBase> components)
        {
            var item = new Accessory
            {
                Type = type,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeAccessory, components.SelectMany(x => x.Effects))
            };
            item.Name = GetItemName("Strength", item);
            return item;
        }

    }
}
