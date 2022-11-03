﻿using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Gameplay.Items;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.SpellsAndGadgets;
using FullPotential.Api.Items.Weapons;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Registry.Crafting;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Elements;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Registry;
using FullPotential.Core.Utilities.Extensions;
using FullPotential.Core.Utilities.Helpers;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Crafting
{
    public class ResultFactory : IResultFactory
    {
        public const int MaxExtraAmmo = 3;

        private readonly TypeRegistry _typeRegistry;
        private readonly ILocalizer _localizer;

        private readonly List<ILoot> _lootTypes;
        private readonly List<IEffect> _effectsForLoot;
        private readonly List<ITargeting> _targetingOptions;
        private readonly List<IShape> _shapeOptions;

        //Cover Names
        private Dictionary<string, string> _coverNamesSog;

        public ResultFactory(
            ITypeRegistry typeRegistry,
            ILocalizer localizer)
        {
            _typeRegistry = (TypeRegistry)typeRegistry;
            _localizer = localizer;

            _lootTypes = _typeRegistry.GetRegisteredTypes<ILoot>().ToList();
            _effectsForLoot = _typeRegistry.GetLootPossibilities();
            _targetingOptions = _typeRegistry.GetRegisteredTypes<ITargeting>().ToList();
            _shapeOptions = _typeRegistry.GetRegisteredTypes<IShape>().ToList();
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

            return result == 0 ? 1 : result;
        }

        public ITargeting GetTargeting(string typeName)
        {
            return _targetingOptions.First(x => x.TypeName == typeName);
        }

        private ITargeting GetTargeting(IEnumerable<IHasTargetingAndShape> components)
        {
            //Exactly one targeting option
            var targetingComponent = components.FirstOrDefault(x => x.Targeting != null);

            if (targetingComponent == null)
            {
                return _typeRegistry.GetRegisteredTypes<ITargeting>().FirstOrDefault();
            }

            return targetingComponent.Targeting;
        }

        public IShape GetShape(string typeName)
        {
            return _shapeOptions.First(x => x.TypeName == typeName);
        }

        private IShape GetShapeOrNone(ITargeting targeting, IEnumerable<IHasTargetingAndShape> components)
        {
            //Only one shape, if any
            if (!targeting.HasShape)
            {
                return null;
            }

            var shapeComponent = components.FirstOrDefault(x => x.Shape != null);

            if (shapeComponent == null)
            {
                return null;
            }

            return _shapeOptions.FirstOrDefault(x => x.TypeId == shapeComponent.Shape.TypeId);
        }

        private List<IEffect> GetEffects(string craftingType, IEnumerable<ItemBase> components, ITargeting targeting = null)
        {
            const string buff = "Buff";
            const string debuff = "Debuff";
            const string other = "Other";

            var effects = components
                .Where(x => x.Effects != null)
                .SelectMany(x => x.Effects)
                .GroupBy(x => x.TypeName)
                .Select(x => x.First());

            var effectTypeLookup = effects
                .ToDictionary(
                    x => x,
                    x =>
                    {
                        if (x is IAttributeEffect attributeEffect)
                        {
                            return attributeEffect.TemporaryMaxIncrease
                                ? buff
                                : debuff;
                        }

                        if (x is not IStatEffect statEffect)
                        {
                            return other;
                        }

                        switch (statEffect.Affect)
                        {
                            case Affect.PeriodicIncrease:
                            case Affect.SingleIncrease:
                            case Affect.TemporaryMaxIncrease:
                                return buff;

                            case Affect.PeriodicDecrease:
                            case Affect.SingleDecrease:
                            case Affect.TemporaryMaxDecrease:
                                return debuff;

                            default:
                                return other;
                        }
                    });

            var elementalEffects = effects.Where(x => x is IElement);
            var elementalEffect = elementalEffects.FirstOrDefault();
            if (elementalEffect != null)
            {
                effects = effects
                    .Except(elementalEffects.Where(x => x != elementalEffect));
            }

            if (craftingType is nameof(Armor) or nameof(Accessory))
            {
                return effects
                    .Where(x =>
                        effectTypeLookup[x] == buff
                        || x is IElement)
                    .ToList();
            }

            if (craftingType == nameof(WeaponItemBase))
            {
                return effects
                    .Where(x =>
                        effectTypeLookup[x] == debuff
                        || x is IElement)
                    .ToList();
            }

            if (craftingType != nameof(Spell) && craftingType != nameof(Gadget))
            {
                throw new Exception($"Unexpected craftingType '{craftingType}'");
            }

            if (effects.Any(x => effectTypeLookup[x] == buff || effectTypeLookup[x] == other))
            {
                effects = effects
                    .Where(x =>
                        effectTypeLookup[x] != debuff
                        && x is not IElement);
            }

            if (targeting != null && !targeting.IsContinuous)
            {
                effects = effects
                    .Where(x =>
                        x is not IMovementEffect movementEffect
                        || movementEffect.Direction != MovementDirection.MaintainDistance);
            }

            return effects.ToList();
        }

        private bool IsSuccess(int percentageChance)
        {
            return ValueCalculator.Random.Next(1, 101) <= percentageChance;
        }

        private int GetAttributeValue(int percentageChance)
        {
            return IsSuccess(percentageChance) ? ValueCalculator.Random.Next(1, 100) : 0;
        }

        private IEffect GetRandomEffect()
        {
            return _effectsForLoot.ElementAt(ValueCalculator.Random.Next(0, _effectsForLoot.Count));
        }

        private ITargeting GetRandomTargetingOrNone()
        {
            if (IsSuccess(50))
            {
                return _targetingOptions.ElementAt(ValueCalculator.Random.Next(0, _targetingOptions.Count));
            }
            return null;
        }

        private IShape GetRandomShapeOrNone()
        {
            if (IsSuccess(10))
            {
                return _shapeOptions.ElementAt(ValueCalculator.Random.Next(0, _shapeOptions.Count));
            }
            return null;
        }

        public ItemBase GetLootDrop()
        {
            var lootDrop = new Loot
            {
                Id = Guid.NewGuid().ToMinimisedString(),
                Attributes = new Attributes
                {
                    IsAutomatic = IsSuccess(50),
                    IsSoulbound = IsSuccess(10),
                    ExtraAmmoPerShot = IsSuccess(20) ? Convert.ToByte(ValueCalculator.Random.Next(1, MaxExtraAmmo + 1)) : (byte)0,
                    Strength = GetAttributeValue(75),
                    Efficiency = GetAttributeValue(75),
                    Range = GetAttributeValue(75),
                    Accuracy = GetAttributeValue(75),
                    Speed = GetAttributeValue(75),
                    Recovery = GetAttributeValue(75),
                    Duration = GetAttributeValue(75),
                    Luck = GetAttributeValue(75)
                }
            };

            var magicalLootTypes = _lootTypes.Where(x => x.Category == ILoot.LootCategory.Magic);
            var techLootTypes = _lootTypes.Where(x => x.Category == ILoot.LootCategory.Technology);

            var isMagical = magicalLootTypes.Any() && IsSuccess(50);
            if (isMagical)
            {
                lootDrop.RegistryType = magicalLootTypes
                    .OrderBy(_ => ValueCalculator.Random.Next())
                    .First();

                var effects = new List<IEffect>();
                var numberOfEffects = MathsHelper.GetMinBiasedNumber(1, Math.Min(4, _effectsForLoot.Count), ValueCalculator.Random);
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

                lootDrop.Effects = effects.ToList();

                lootDrop.Targeting = GetRandomTargetingOrNone();
                lootDrop.Shape = GetRandomShapeOrNone();
            }
            else
            {
                lootDrop.RegistryType = techLootTypes
                    .OrderBy(_ => ValueCalculator.Random.Next())
                    .First();
            }

            var typeTranslation = _localizer.Translate("crafting.loot.type");
            var suffix = int.Parse(lootDrop.GetNameHash().ToString().TrimStart('-').Substring(5));

            lootDrop.Name = $"{_localizer.GetTranslatedTypeName(lootDrop.RegistryType)} ({typeTranslation} #{suffix.ToString("D5", GameManager.Instance.CurrentCulture)})";

            return lootDrop;
        }

        private SpellOrGadgetItemBase GetSpellOrGadget(string categoryName, IEnumerable<ItemBase> components)
        {
            var relevantComponents = components.OfType<IHasTargetingAndShape>();

            var targeting = GetTargeting(relevantComponents);

            SpellOrGadgetItemBase spellOrGadget;
            if (categoryName == nameof(Spell))
            {
                spellOrGadget = new Spell();
            }
            else
            {
                spellOrGadget = new Gadget();
            }

            spellOrGadget.Id = Guid.NewGuid().ToMinimisedString();
            spellOrGadget.Targeting = targeting;
            spellOrGadget.Shape = GetShapeOrNone(targeting, relevantComponents);
            spellOrGadget.Attributes = new Attributes
            {
                IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                Range = ComputeAttribute(components, x => x.Attributes.Range),
                Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                Recovery = ComputeAttribute(components, x => x.Attributes.Recovery),
                Duration = ComputeAttribute(components, x => x.Attributes.Duration)
            };
            spellOrGadget.Effects = GetEffects(categoryName, components, targeting);

            var suffix = _localizer.Translate(TranslationType.CraftingCategory, categoryName);

            if (spellOrGadget.Effects.Count > 0)
            {
                spellOrGadget.Name = _localizer.GetTranslatedTypeName(spellOrGadget.Effects.First()) + " " + suffix;
            }
            else
            {
                var customSuffix = _localizer.GetTranslatedTypeName(spellOrGadget.Targeting) + " " + suffix;
                spellOrGadget.Name = GetItemName(true, spellOrGadget, customSuffix);
            }

            return spellOrGadget;
        }

        private string GetItemNamePrefix(bool isAttack)
        {
            return _localizer.Translate(TranslationType.CraftingNamePrefix, isAttack ? "attack" : "defence");
        }

        private string GetItemName(bool isAttack, ItemBase item, string customSuffix = null)
        {
            var suffix = string.IsNullOrWhiteSpace(customSuffix)
                ? _localizer.GetTranslatedTypeName(item.RegistryType)
                : customSuffix;
            return $"{GetItemNamePrefix(isAttack)} {item.Attributes.Strength} {suffix}";
        }

        private WeaponItemBase GetMeleeWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new MeleeWeapon
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToMinimisedString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed)
                },
                Effects = GetEffects(nameof(WeaponItemBase), components)
            };
            weapon.Name = GetItemName(true, weapon);
            return weapon;
        }

        private WeaponItemBase GetRangedWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new RangedWeapon
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToMinimisedString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsAutomatic = craftableType.AllowAutomatic && components.Any(x => x.Attributes.IsAutomatic),
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    ExtraAmmoPerShot = components.FirstOrDefault(x => x.Attributes.ExtraAmmoPerShot > 0)?.Attributes.ExtraAmmoPerShot ?? 0,
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(nameof(WeaponItemBase), components)
            };
            weapon.Name = GetItemName(true, weapon);
            weapon.Ammo = weapon.GetAmmoMax();
            return weapon;
        }

        private WeaponItemBase GetDefensiveWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new DefensiveWeapon
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToMinimisedString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(nameof(WeaponItemBase), components)
            };
            weapon.Name = GetItemName(false, weapon);
            return weapon;
        }

        private Armor GetArmor(IGearArmor craftableType, IEnumerable<ItemBase> components)
        {
            var armor = new Armor
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToMinimisedString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(nameof(Armor), components)
            };
            armor.Name = GetItemName(false, armor);
            return armor;
        }

        private Armor GetBarrier(IGearArmor craftableType, IEnumerable<ItemBase> components)
        {
            var armor = new Armor
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToMinimisedString(),
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
            armor.Name = GetItemName(false, armor);
            return armor;
        }

        private Accessory GetAccessory(IGearAccessory craftableType, IEnumerable<ItemBase> components)
        {
            var accessory = new Accessory
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToMinimisedString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(nameof(Accessory), components)
            };
            accessory.Name = GetItemName(true, accessory);
            return accessory;
        }

        public ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IEnumerable<ItemBase> components)
        {
            if (categoryName == nameof(Spell) || categoryName == nameof(Gadget))
            {
                return GetSpellOrGadget(categoryName, components);
            }

            switch (categoryName)
            {
                case nameof(WeaponItemBase):
                    var craftableWeapon = _typeRegistry.GetRegisteredByTypeName<IGearWeapon>(typeName);
                    switch (craftableWeapon.Category)
                    {
                        case IGearWeapon.WeaponCategory.Melee: return GetMeleeWeapon(craftableWeapon, components, isTwoHanded);
                        case IGearWeapon.WeaponCategory.Ranged: return GetRangedWeapon(craftableWeapon, components, isTwoHanded);
                        case IGearWeapon.WeaponCategory.Defensive: return GetDefensiveWeapon(craftableWeapon, components, isTwoHanded);
                        default: throw new Exception($"Unexpected weapon category '{craftableWeapon.Category}'");
                    }

                case nameof(Armor):
                    var craftableArmor = _typeRegistry.GetRegisteredByTypeName<IGearArmor>(typeName);
                    return craftableArmor.Category == IGearArmor.ArmorCategory.Barrier
                        ? GetBarrier(craftableArmor, components)
                        : GetArmor(craftableArmor, components);

                case nameof(Accessory):
                    var craftableAccessory = _typeRegistry.GetRegisteredByTypeName<IGearAccessory>(typeName);
                    return GetAccessory(craftableAccessory, components);

                default:
                    throw new Exception($"Unexpected craftable category '{categoryName}'");
            }
        }

    }
}
