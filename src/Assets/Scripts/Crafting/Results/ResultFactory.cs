using Assets.Scripts.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Crafting.Results
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
                Spell.BuffRegen,
                Spell.BuffHaste,
                Spell.BuffCourage,
                Spell.BuffFocus,
                Spell.BuffStrengthen,
                Spell.BuffLifeTap,
                Spell.BuffManaTap
            };

            _debuffEffects = new List<string>
            {
                Spell.DebuffPoison,
                Spell.DebuffSlow,
                Spell.DebuffFear,
                Spell.DebuffDistract,
                Spell.DebuffWeaken,
                Spell.DebuffLifeDrain,
                Spell.DebuffManaDrain
            };

            _supportEffects = new List<string>
            {
                Spell.SupportHeal,
                Spell.SupportLeap,
                Spell.SupportBlink,
                Spell.SupportSoften,
                Spell.SupportAbsorb,
                Spell.SupportDeflect
            };

            _damageEffects = new List<string>
            {
                Spell.DamageForce,
                Spell.DamageFire,
                Spell.DamageLightning,
                Spell.DamageIce,
                Spell.DamageEarth,
                Spell.DamageWater,
                Spell.DamageAir
            };

            _lingeringEffects = new List<string>
            {
                Spell.LingeringIgnition,
                Spell.LingeringShock,
                Spell.LingeringFreeze,
                Spell.LingeringImmobilise
            };

            _lingeringPairing = new Dictionary<string, string>
            {
                { Spell.LingeringIgnition, Spell.DamageFire },
                { Spell.LingeringShock, Spell.DamageLightning },
                { Spell.LingeringFreeze, Spell.DamageIce },
                { Spell.LingeringImmobilise, Spell.DamageEarth }
            };

            _targetingEffects = new List<string>
            {
                Spell.TargetingSelf,
                Spell.TargetingTouch,
                Spell.TargetingProjectile,
                Spell.TargetingBeam,
                Spell.TargetingCone
            };

            _shapeEffects = new List<string>
            {
                Spell.ShapeZone,
                Spell.ShapeWall
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

            if (result == 0)
            {
                result = 1;
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
            var effects = effectsInput.Except(new[] { Spell.BuffLifeTap, Spell.BuffManaTap });

            //If there is a buff or support then remove all debuffs
            if (effects.Intersect(_buffEffects).Any() || effects.Intersect(_supportEffects).Any())
            {
                effects = effects.Except(_debuffEffects);
            }

            if (craftingType == ChooseCraftingType.CraftingTypeArmor || craftingType == ChooseCraftingType.CraftingTypeAccessory)
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
            if (craftingType == ChooseCraftingType.CraftingTypeWeapon)
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
                if (shapeEffects.Contains(Spell.TargetingBeam) || shapeEffects.Contains(Spell.TargetingCone))
                {
                    effects = effects.Except(_shapeEffects);
                }
                else
                {
                    effects = effects.Except(_shapeEffects.Where(x => x != shapeEffects.Last()));
                }
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
            return _allEffects.ElementAt(_random.Next(0, _allEffects.Count - 1));
        }

        private int GetBiasedNumber(int min, int max)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(_random.NextDouble(), 3), 0);
        }

        internal CraftableBase GetLootDrop()
        {
            //todo: limit good drops to higher level players

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
        //todo: add a min level

        internal CraftableBase GetSpell(List<CraftableBase> components)
        {
            return new Spell
            {
                Name = "Unnamed Spell",
                Targeting = Spell.TargetingProjectile, //todo: set this
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
                Effects = GetEffects(ChooseCraftingType.CraftingTypeSpell, components.SelectMany(x => x.Effects)),
                //todo: Shape = 
            };
        }

        internal CraftableBase GetMeleeWeapon(string type, List<CraftableBase> components, bool isTwoHanded)
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
                Effects = GetEffects(ChooseCraftingType.CraftingTypeWeapon, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase GetRangedWeapon(string type, List<CraftableBase> components, bool isTwoHanded)
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
                Effects = GetEffects(ChooseCraftingType.CraftingTypeWeapon, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase GetShield(List<CraftableBase> components)
        {
            return new Weapon
            {
                Name = "Unnamed Shield",
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
        }

        internal CraftableBase GetArmor(string type, List<CraftableBase> components)
        {
            return new Armor
            {
                Name = "Unnamed Armor",
                Type = type,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeArmor, components.SelectMany(x => x.Effects))
            };
        }

        internal CraftableBase GetBarrier(List<CraftableBase> components)
        {
            return new Armor
            {
                Name = "Unnamed Barrier",
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
        }

        internal CraftableBase GetAccessory(string type, List<CraftableBase> components)
        {
            return new Accessory
            {
                Name = "Unnamed Accessory",
                Type = type,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(ChooseCraftingType.CraftingTypeAccessory, components.SelectMany(x => x.Effects))
            };
        }

    }
}
