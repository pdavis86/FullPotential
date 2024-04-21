using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Localization;
using FullPotential.Api.Localization.Enums;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.Registry;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Crafting
{
    public class ResultFactory : IResultFactory
    {
        public const int MaxExtraAmmo = 3;

        private readonly Random _random = new Random();
        private readonly TypeRegistry _typeRegistry;
        private readonly ILocalizer _localizer;

        private readonly List<ILootType> _lootTypes;
        private readonly List<IAmmunitionType> _ammoTypes;
        private readonly List<ITargetingType> _targetingTypes;
        private readonly List<IShapeType> _shapeTypes;
        private readonly List<IEffectType> _effectsForLoot;

        public ResultFactory(
            ITypeRegistry typeRegistry,
            ILocalizer localizer)
        {
            _typeRegistry = (TypeRegistry)typeRegistry;
            _localizer = localizer;

            _lootTypes = _typeRegistry.GetRegisteredTypes<ILootType>().ToList();
            _ammoTypes = _typeRegistry.GetRegisteredTypes<IAmmunitionType>().ToList();
            _targetingTypes = _typeRegistry.GetRegisteredTypes<ITargetingType>().ToList();
            _shapeTypes = _typeRegistry.GetRegisteredTypes<IShapeType>().ToList();

            _effectsForLoot = _typeRegistry.GetRegisteredTypes<IEffectType>()
                .Where(x => x is not IIsSideEffect)
                .ToList();
        }

        private int ComputeAttribute(IList<CombatItemBase> components, Func<CombatItemBase, int> getProp, bool allowMax = true)
        {
            var withValue = components.Where(x => getProp(x) > 0).ToList();

            if (!withValue.Any())
            {
                return 1;
            }

            var min = withValue.Min(getProp);
            var max = withValue.Max(getProp);
            var topEndSkew = max - ((max - min) / 10);

            var result = allowMax
                ? topEndSkew
                : (int)Math.Round(topEndSkew - (0.009 * (topEndSkew - 50)), MidpointRounding.AwayFromZero);

            return result == 0 ? 1 : result;
        }

        private ITargetingType GetTargeting(IList<IHasTargetingAndShape> components)
        {
            //Exactly one targeting option
            var targetingComponent = components.FirstOrDefault(x => x.Targeting != null);

            if (targetingComponent == null)
            {
                return _typeRegistry.GetRegisteredTypes<ITargetingType>().FirstOrDefault();
            }

            return targetingComponent.Targeting;
        }

        private IShapeType GetShapeOrNone(ITargetingType targeting, IList<IHasTargetingAndShape> components)
        {
            //Only one shape, if any
            if (!targeting.CanHaveShape)
            {
                return null;
            }

            var shapeComponent = components.FirstOrDefault(x => x.Shape != null);

            if (shapeComponent == null)
            {
                return null;
            }

            return _shapeTypes.FirstOrDefault(x => x.TypeId == shapeComponent.Shape.TypeId);
        }

        private List<IEffectType> GetEffects(CraftableType craftableType, IList<CombatItemBase> components, ITargetingType targeting = null)
        {
            const string buff = "Buff";
            const string debuff = "Debuff";
            const string other = "Other";

            var effects = components
                .Where(x => x.Effects != null)
                .SelectMany(x => x.Effects)
                .GroupBy(x => x.TypeId)
                .Select(x => x.First());

            var effectTypeLookup = effects
                .ToDictionary(
                    x => x,
                    x =>
                    {
                        if (x is IAttributeEffect attributeEffect)
                        {
                            return attributeEffect.IsTemporaryMaxIncrease
                                ? buff
                                : debuff;
                        }

                        if (x is IResourceEffectType resourceEffect)
                        {
                            switch (resourceEffect.EffectActionType)
                            {
                                case EffectActionType.PeriodicIncrease:
                                case EffectActionType.SingleIncrease:
                                case EffectActionType.TemporaryMaxIncrease:
                                    return buff;

                                case EffectActionType.PeriodicDecrease:
                                case EffectActionType.SingleDecrease:
                                case EffectActionType.TemporaryMaxDecrease:
                                    return debuff;

                                default:
                                    return other;
                            }
                        }

                        return other;
                    });

            if (craftableType is CraftableType.Armor or CraftableType.Accessory)
            {
                return effects
                    .Where(x => effectTypeLookup[x] == buff)
                    .ToList();
            }

            if (craftableType == CraftableType.Weapon)
            {
                return effects
                    .Where(x => effectTypeLookup[x] == debuff)
                    .ToList();
            }

            if (craftableType != CraftableType.Consumer)
            {
                throw new Exception($"Unexpected craftingType '{craftableType}'");
            }

            if (effects.Any(x => effectTypeLookup[x] == buff))
            {
                effects = effects
                    .Where(x => effectTypeLookup[x] != debuff);
            }

            if (effects.Any(x => effectTypeLookup[x] == debuff))
            {
                effects = effects
                    .Where(x => effectTypeLookup[x] != buff);
            }

            if (targeting != null && !targeting.IsContinuous)
            {
                effects = effects
                    .Where(x =>
                        x is not IMovementEffectType movementEffect
                        || movementEffect.Direction != MovementDirection.MaintainDistance);
            }

            return effects.ToList();
        }

        private bool IsSuccess(int percentageChance)
        {
            return _random.Next(1, 101) <= percentageChance;
        }

        private int GetAttributeValue(int percentageChance)
        {
            //todo: zzz v0.6 - Replace all hard-coded 100 values

            return IsSuccess(percentageChance) ? _random.Next(1, 100) : 0;
        }

        private IEffectType GetRandomEffect()
        {
            return _effectsForLoot.ElementAt(_random.Next(0, _effectsForLoot.Count));
        }

        private ITargetingType GetRandomTargetingOrNone()
        {
            if (IsSuccess(50))
            {
                return _targetingTypes.ElementAt(_random.Next(0, _targetingTypes.Count));
            }
            return null;
        }

        private IShapeType GetRandomShapeOrNone()
        {
            if (IsSuccess(10))
            {
                return _shapeTypes.ElementAt(_random.Next(0, _shapeTypes.Count));
            }
            return null;
        }

        public ItemBase GetLootDrop()
        {
            var lootDrop = new Loot
            {
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsAutomatic = IsSuccess(50),
                    ExtraAmmoPerShot = IsSuccess(20) ? Convert.ToByte(_random.Next(1, MaxExtraAmmo + 1)) : (byte)0,
                    Strength = GetAttributeValue(75),
                    Efficiency = GetAttributeValue(75),
                    Range = GetAttributeValue(75),
                    Accuracy = GetAttributeValue(75),
                    Speed = GetAttributeValue(75),
                    Recovery = GetAttributeValue(75),
                    Duration = GetAttributeValue(75),
                    Luck = GetAttributeValue(75)
                },
                RegistryType = _lootTypes
                    .OrderBy(_ => _random.Next())
                    .First()
            };

            if (IsSuccess(50))
            {
                var effects = new List<IEffectType>();
                var numberOfEffects = GetMinBiasedNumber(1, Math.Min(4, _effectsForLoot.Count));
                for (var i = 1; i <= numberOfEffects; i++)
                {
                    IEffectType effect;
                    var debugCounter = 0;

                    do
                    {
                        effect = GetRandomEffect();
                        debugCounter++;
                    }
                    while (debugCounter < 10 && (effect == null || effects.Contains(effect)));

                    if (debugCounter >= 10)
                    {
                        UnityEngine.Debug.LogError("Infinite loop situation here. Go fix it!");
                    }

                    effects.Add(effect);
                }

                lootDrop.Effects = effects.ToList();

                lootDrop.Targeting = GetRandomTargetingOrNone();
                lootDrop.Shape = GetRandomShapeOrNone();
            }


            var typeTranslation = _localizer.Translate("crafting.loot.type");
            var suffix = int.Parse(lootDrop.GetNameHash().ToString().TrimStart('-').Substring(5));

            lootDrop.Name = $"{_localizer.Translate(lootDrop.RegistryType)} ({typeTranslation} #{suffix.ToString("D5", _localizer.CurrentCulture)})";

            return lootDrop;
        }

        public ItemBase GetAmmoDrop()
        {
            var randomAmmo = _ammoTypes
                .OrderBy(_ => _random.Next())
                .First();

            var randomCount = _random.Next(randomAmmo.MinDropCount, randomAmmo.MaxDropCount);

            return new ItemStack
            {
                RegistryType = randomAmmo,
                Id = Guid.NewGuid().ToString(),
                BaseName = _localizer.Translate(randomAmmo),
                Count = randomCount
            };
        }

        private int GetMinBiasedNumber(int min, int max)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(_random.NextDouble(), 3), 0);
        }

        private Consumer GetConsumer(CraftableType craftableType, string resourceTypeId, IList<CombatItemBase> components)
        {
            var relevantComponents = components.OfType<IHasTargetingAndShape>().ToList();

            var targeting = GetTargeting(relevantComponents);

            var resourceType = _typeRegistry.GetRegisteredByTypeId<IResourceType>(resourceTypeId);

            var consumer = new Consumer
            {
                Id = Guid.NewGuid().ToString(),
                ResourceType = resourceType,
                Targeting = targeting,
                Shape = GetShapeOrNone(targeting, relevantComponents),
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
                Effects = GetEffects(craftableType, components, targeting)
            };

            var suffix = _localizer.Translate(consumer.Targeting)
                + (consumer.Shape != null ? " " + _localizer.Translate(consumer.Shape) : null)
                + " " + _localizer.Translate(TranslationType.ItemType, nameof(Consumer));

            if (consumer.Effects.Count > 0)
            {
                consumer.Name = _localizer.Translate(consumer.Effects.First()) + " " + suffix;
            }
            else
            {
                consumer.Name = GetItemName(true, consumer, suffix);
            }

            return consumer;
        }

        private string GetItemNamePrefix(bool isAttack)
        {
            return _localizer.Translate(TranslationType.CraftingNamePrefix, isAttack ? "attack" : "defence");
        }

        private string GetItemName(bool isAttack, CombatItemBase item, string customSuffix = null)
        {
            var suffix = string.IsNullOrWhiteSpace(customSuffix)
                ? _localizer.Translate(item.RegistryType)
                : customSuffix;
            return $"{GetItemNamePrefix(isAttack)} {item.Attributes.Strength} {suffix}";
        }

        private Weapon GetMeleeWeapon(IWeaponType craftableType, IList<CombatItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(CraftableType.Weapon, components)
            };
            weapon.Name = GetItemName(true, weapon);
            return weapon;
        }

        private Weapon GetRangedWeapon(IWeaponType craftableType, IList<CombatItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsAutomatic = craftableType.AllowAutomatic && components.Any(x => x.Attributes.IsAutomatic),
                    ExtraAmmoPerShot = components.FirstOrDefault(x => x.Attributes.ExtraAmmoPerShot > 0)?.Attributes.ExtraAmmoPerShot ?? 0,
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(CraftableType.Weapon, components)
            };
            weapon.Name = GetItemName(true, weapon);
            weapon.Ammo = weapon.GetAmmoMax();
            return weapon;
        }

        private Weapon GetDefensiveWeapon(IWeaponType craftableType, IList<CombatItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(CraftableType.Weapon, components)
            };
            weapon.Name = GetItemName(false, weapon);
            return weapon;
        }

        private Armor GetArmor(IArmorType craftableType, IList<CombatItemBase> components)
        {
            var armor = new Armor
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(CraftableType.Armor, components)
            };
            armor.Name = GetItemName(false, armor);
            return armor;
        }

        private Accessory GetAccessory(IAccessoryType craftableType, IList<CombatItemBase> components)
        {
            var accessory = new Accessory
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Name = craftableType.GetType().Name.ToSpacedString(),
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(CraftableType.Accessory, components)
            };
            return accessory;
        }

        private SpecialGear GetSpecialGear(ISpecialGearType craftableType, string resourceTypeId, IList<CombatItemBase> components)
        {
            var resourceType = resourceTypeId.IsNullOrWhiteSpace()
                ? null
                : _typeRegistry.GetRegisteredByTypeId<IResourceType>(resourceTypeId);

            var specialGear = new SpecialGear
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Name = craftableType.GetType().Name.ToSpacedString(),
                ResourceType = resourceType,
                Attributes = new Attributes
                {
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery),
                    Duration = ComputeAttribute(components, x => x.Attributes.Duration),
                    Luck = ComputeAttribute(components, x => x.Attributes.Luck),
                },
            };

            return specialGear;
        }

        public ItemBase GetCraftedItem(CraftableType craftableType, string typeId, string resourceTypeId, bool isTwoHanded, IList<CombatItemBase> components)
        {
            switch (craftableType)
            {
                case CraftableType.Consumer:
                    return GetConsumer(craftableType, resourceTypeId, components);

                case CraftableType.Weapon:
                    var weaponType = _typeRegistry.GetRegisteredByTypeId<IWeaponType>(typeId);
                    if (weaponType.IsDefensive)
                    {
                        return GetDefensiveWeapon(weaponType, components, isTwoHanded);
                    }

                    if (weaponType.AmmunitionTypeIdString == null)
                    {
                        return GetMeleeWeapon(weaponType, components, isTwoHanded);
                    }

                    return GetRangedWeapon(weaponType, components, isTwoHanded);

                case CraftableType.Armor:
                    var craftableArmor = _typeRegistry.GetRegisteredByTypeId<IArmorType>(typeId);
                    return GetArmor(craftableArmor, components);

                case CraftableType.Accessory:
                    var craftableAccessory = _typeRegistry.GetRegisteredByTypeId<IAccessoryType>(typeId);
                    return GetAccessory(craftableAccessory, components);

                case CraftableType.SpecialGear:
                    var craftableSpecial = _typeRegistry.GetRegisteredByTypeId<ISpecialGearType>(typeId);
                    return GetSpecialGear(craftableSpecial, resourceTypeId, components);

                default:
                    throw new Exception($"Unexpected craftable category '{craftableType}'");
            }
        }

    }
}
