using Assets.Scripts.Attributes;
using Assets.Scripts.Ui.Crafting.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Ui.Crafting
{
    [ServerSideOnlyTemp]
    public class ResultFactory
    {
        private readonly System.Random _random;
        private readonly List<string> _buffEffects;
        private readonly List<string> _debuffEffects;
        private readonly List<string> _supportEffects;
        private readonly List<string> _damageEffects;
        private readonly List<string> _lingeringEffects;
        private readonly Dictionary<string, string> _lingeringPairing;
        private readonly List<string> _targetingEffects;
        private readonly List<string> _shapeEffects;
        private readonly List<string> _allEffects;

        public ResultFactory()
        {
            _random = new System.Random();
            _buffEffects = new List<string>
            {
                Items.Spell.BuffRegen,
                Items.Spell.BuffHaste,
                Items.Spell.BuffCourage,
                Items.Spell.BuffFocus,
                Items.Spell.BuffStrengthen,
                Items.Spell.BuffLifeTap,
                Items.Spell.BuffManaTap
            };

            _debuffEffects = new List<string>
            {
                Items.Spell.DebuffPoison,
                Items.Spell.DebuffSlow,
                Items.Spell.DebuffFear,
                Items.Spell.DebuffDistract,
                Items.Spell.DebuffWeaken,
                Items.Spell.DebuffLifeDrain,
                Items.Spell.DebuffManaDrain
            };

            _supportEffects = new List<string>
            {
                Items.Spell.SupportHeal,
                Items.Spell.SupportLeap,
                Items.Spell.SupportBlink,
                Items.Spell.SupportSoften,
                Items.Spell.SupportAbsorb,
                Items.Spell.SupportDeflect
            };

            _damageEffects = new List<string>
            {
                Items.Spell.DamageForce,
                Items.Spell.DamageFire,
                Items.Spell.DamageLightning,
                Items.Spell.DamageIce,
                Items.Spell.DamageEarth,
                Items.Spell.DamageWater,
                Items.Spell.DamageAir
            };

            _lingeringEffects = new List<string>
            {
                Items.Spell.LingeringIgnition,
                Items.Spell.LingeringShock,
                Items.Spell.LingeringFreeze,
                Items.Spell.LingeringImmobilise
            };

            _lingeringPairing = new Dictionary<string, string>
            {
                { Items.Spell.LingeringIgnition, Items.Spell.DamageFire },
                { Items.Spell.LingeringShock, Items.Spell.DamageLightning },
                { Items.Spell.LingeringFreeze, Items.Spell.DamageIce },
                { Items.Spell.LingeringImmobilise, Items.Spell.DamageEarth }
            };

            _targetingEffects = new List<string>
            {
                Items.Spell.TargetingSelf,
                Items.Spell.TargetingTouch,
                Items.Spell.TargetingProjectile,
                Items.Spell.TargetingBeam,
                Items.Spell.TargetingCone
            };

            _shapeEffects = new List<string>
            {
                Items.Spell.ShapeZone,
                Items.Spell.ShapeWall
            };

            _allEffects = _buffEffects
                .Union(_debuffEffects)
                .Union(_supportEffects)
                .Union(_damageEffects)
                //Do not include as it needs pairing: .Union(_lingeringEffects)
                .Union(_targetingEffects)
                .Union(_shapeEffects)
                .ToList();
        }

        private int GetValue(int rarityThreshold)
        {
            return _random.Next(0, 100) > rarityThreshold ? _random.Next(1, 100) : 0;
        }

        private int ComputeAttribute(List<CraftableBase> components, Func<CraftableBase, int> getProp)
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

            //Debug.Log($"{getProp.Method.Name} = Min:{min}, Max:{max}, Skew:{topEndSkew}, Result:{result}");

            return result;
        }

        private int PickValueAtRandom(List<CraftableBase> components, Func<CraftableBase, int> getProp)
        {
            var values = components.Select(getProp);
            var takeAt = _random.Next(0, values.Count() - 1);
            return values.ElementAt(takeAt);
        }

        private List<string> GetEffects(string craftingType, IEnumerable<string> effectsInput)
        {
            //Cannot cast "tap" buffs
            var effects = effectsInput.Except(new[] { Items.Spell.BuffLifeTap, Items.Spell.BuffManaTap });

            //If there is a buff or support then remove all debuffs
            if (effects.Intersect(_buffEffects).Any() || effects.Intersect(_supportEffects).Any())
            {
                effects = effects.Except(_debuffEffects);
            }

            if (craftingType == ChooseCraftingType.Armor || craftingType == ChooseCraftingType.Accessory)
            {
                return effects.Intersect(_buffEffects)
                    .Union(effects.Intersect(_supportEffects))
                    .ToList();
            }

            //Lingering must have matching damage type
            var damageEffects = effects.Intersect(_damageEffects);
            var lingering = effects.Intersect(_lingeringEffects);
            if (lingering.Any())
            {
                var damageFound = false;
                foreach (var effect in lingering)
                {
                    var expectedDamageType = _lingeringPairing[effect];
                    if (damageEffects.Contains(expectedDamageType))
                    {
                        damageFound = true;
                        effects = effects.Except(_lingeringEffects.Where(x => x != effect));
                        effects = effects.Except(_damageEffects.Where(x => x != expectedDamageType));
                    }
                    break;
                }
                if (!damageFound)
                {
                    effects = effects.Except(_lingeringEffects);
                }
            }

            //Remove all but the last damage effect
            damageEffects = effects.Intersect(_damageEffects);
            if (damageEffects.Count() > 1)
            {
                effects = effects.Except(_damageEffects.Where(x => x != damageEffects.Last()));
            }

            //Weapons to dame and debuffs only
            if (craftingType == ChooseCraftingType.Weapon)
            {
                return effects.Intersect(_debuffEffects)
                    .Union(effects.Intersect(_damageEffects))
                    .Union(effects.Intersect(_lingeringEffects))
                    .ToList();
            }

            //Only one target
            var targetEffects = effects.Intersect(_targetingEffects);
            if (targetEffects.Count() > 1)
            {
                effects = effects.Except(_targetingEffects.Where(x => x != targetEffects.Last()));
            }

            //Only one valid shape
            var shapeEffects = effects.Intersect(_shapeEffects);
            if (shapeEffects.Any())
            {
                if (shapeEffects.Contains(Items.Spell.TargetingBeam) || shapeEffects.Contains(Items.Spell.TargetingCone))
                {
                    effects = effects.Except(_shapeEffects);
                }
                else
                {
                    effects = effects.Except(_shapeEffects.Where(x => x != shapeEffects.Last()));
                }
            }

            if (craftingType != ChooseCraftingType.Spell)
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
            return _allEffects.ElementAt(_random.Next(0, _allEffects.Count - 1));
        }

        private int GetBiasedNumber(int min, int max)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(_random.NextDouble(), 3), 0);
        }

        internal CraftableBase GetLootDrop()
        {
            var lootDrop = new CraftableBase
            {
                Attributes = GetRandomAttributes(),
                Effects = new List<string>()
            };

            var isMagical = _random.Next(0, 2) > 0;
            if (isMagical)
            {
                var numberOfEffects = GetBiasedNumber(1, 4);
                for (var i = 1; i <= numberOfEffects; i++)
                {
                    string effect;
                    do { effect = GetRandomEffect(); }
                    while (lootDrop.Effects.Contains(effect));
                    lootDrop.Effects.Add(effect);
                }

                Debug.Log($"Added {numberOfEffects} effects: {string.Join(", ", lootDrop.Effects)}");
            }

            return lootDrop;
        }

        //todo: add ability to name item
        //todo: add validation e.g. enough scrap to make a two-handed weapon
        //todo: add validation e.g. at least one effect for a spell

        internal CraftableBase Spell(List<CraftableBase> components)
        {
            return new Spell
            {
                Name = "Unnamed Spell",
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
                Effects = GetEffects(ChooseCraftingType.Spell, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase MeleeWeapon(string type, List<CraftableBase> components, bool isTwoHanded)
        {
            return new Weapon
            {
                Name = "Unnamed Melee Weapon",
                Type = type,
                IsTwoHanded = isTwoHanded,
                Attributes = new Attributes
                {
                    IsActivated = components.Any(x => x.Attributes.IsActivated),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed)
                },
                Effects = GetEffects(ChooseCraftingType.Weapon, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase RangedWeapon(string type, List<CraftableBase> components, bool isTwoHanded)
        {
            return new Weapon
            {
                Name = "Unnamed Ranged Weapon",
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
                Effects = GetEffects(ChooseCraftingType.Weapon, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase Shield(List<CraftableBase> components)
        {
            return new Weapon
            {
                Name = "Unnamed Shield",
                Type = Items.Weapon.Shield,
                Attributes = new Attributes
                {
                    IsActivated = components.Any(x => x.Attributes.IsActivated),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(ChooseCraftingType.Weapon, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase Armor(string type, List<CraftableBase> components)
        {
            return new Armor
            {
                Name = "Unnamed Armor",
                Type = type,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(ChooseCraftingType.Armor, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase Barrier(List<CraftableBase> components)
        {
            return new Armor
            {
                Name = "Unnamed Barrier",
                Type = Items.Armor.Barrier,
                Attributes = new Attributes
                {
                    IsActivated = components.Any(x => x.Attributes.IsActivated),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Cost = ComputeAttribute(components, x => x.Attributes.Cost),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(ChooseCraftingType.Armor, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase Accessory(string type, List<CraftableBase> components)
        {
            return new Accessory
            {
                Name = "Unnamed Accessory",
                Type = type,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(ChooseCraftingType.Accessory, components.SelectMany(x => x.Effects))
            };
        }

    }
}
