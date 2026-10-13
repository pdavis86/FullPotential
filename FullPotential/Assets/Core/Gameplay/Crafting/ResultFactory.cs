using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Gameplay.Crafting;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Localization;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
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
        private readonly IAuditor _logger;
        private readonly TypeRegistry _typeRegistry;
        private readonly ILocalizer _localizer;

        private readonly List<ILootType> _lootTypes;
        private readonly List<IAmmunitionType> _ammoTypes;
        private readonly List<ITargetingType> _targetingTypes;
        private readonly List<IShapeType> _shapeTypes;
        private readonly List<IEffectType> _effectsForLoot;

        public ResultFactory(
            IAuditorFactory auditorFactory,
            ITypeRegistry typeRegistry,
            ILocalizer localizer)
        {
            _logger = auditorFactory.Create(this);
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

        public int ComputeAttribute(IList<ItemBase> components, string attributeId, bool allowMax = true)
        {
            int getProp(ItemBase x)
            {
                return x.Attributes.GetIntValue(attributeId);
            }

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

        public void SetTargetingAndShape(IHasTargetingAndShape item, IList<ItemBase> components)
        {
            var relevantComponents = components.OfType<IHasTargetingAndShape>().ToList();

            var targetingComponent = relevantComponents.FirstOrDefault(x => x.TargetingType != null);

            item.TargetingType = targetingComponent == null
                ? _typeRegistry.GetRegisteredTypes<ITargetingType>().FirstOrDefault()
                : targetingComponent.TargetingType;

            if (item.TargetingType == null || !item.TargetingType.CanHaveShape)
            {
                return;
            }

            var shapeComponent = relevantComponents.FirstOrDefault(x => x.ShapeType != null);

            if (shapeComponent == null)
            {
                return;
            }

            item.ShapeType = _shapeTypes.FirstOrDefault(x => x.TypeId == shapeComponent.ShapeType.TypeId);
        }

        public void SetEffects(IHasEffects item, IList<ItemBase> components)
        {
            const string buff = "Buff";
            const string debuff = "Debuff";
            const string other = "Other";

            var relevantComponents = components.OfType<IHasEffects>().ToList();

            var effects = relevantComponents
                .Where(x => x.EffectTypes != null)
                .SelectMany(x => x.EffectTypes)
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
                            }
                        }

                        return other;
                    });

            var itemBase = (ItemBase)item;

            if (itemBase.RegistryType is IArmorType or IAccessoryType)
            {
                item.EffectTypes = effects
                    .Where(x => effectTypeLookup[x] == buff)
                    .ToList();
            }

            if (itemBase.RegistryType is IWeaponType)
            {
                item.EffectTypes = effects
                    .Where(x => effectTypeLookup[x] == debuff)
                    .ToList();
            }

            if (item is not Consumer consumer)
            {
                return;
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

            if (consumer.TargetingType != null && !consumer.TargetingType.IsContinuous)
            {
                effects = effects
                    .Where(x =>
                        x is not IMovementEffectType movementEffect
                        || movementEffect.Direction != MovementDirection.MaintainDistance);
            }

            item.EffectTypes = effects.ToList();
        }

        private bool IsSuccess(int percentageChance)
        {
            return _random.Next(1, 101) <= percentageChance;
        }

        private int GetAttributeValue(int percentageChance)
        {
            //todo: Replace all hard-coded 100 values

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
            var lootDrop = _typeRegistry.GetItemTypeMapping<ILootType>().CreateNew();

            // todo: loot tables
            //var lootDrop = new Loot
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Attributes = new Dictionary<string, string>
            //    {
            //        { "IsAutomatic", IsSuccess(50).ToString() },
            //        { "ExtraAmmoPerShot", (IsSuccess(20) ? Convert.ToByte(_random.Next(1, MaxExtraAmmo + 1)) : (byte)0).ToString() },
            //        { "Strength", GetAttributeValue(75).ToString() },
            //        { "Efficiency", GetAttributeValue(75).ToString() },
            //        { "Range", GetAttributeValue(75).ToString() },
            //        { "Accuracy", GetAttributeValue(75).ToString() },
            //        { "Speed", GetAttributeValue(75).ToString() },
            //        { "Recovery", GetAttributeValue(75).ToString() },
            //        { "Duration", GetAttributeValue(75).ToString() },
            //        { "Luck", GetAttributeValue(75).ToString() },
            //    },
            //    RegistryType = _lootTypes
            //        .OrderBy(_ => _random.Next())
            //        .First()
            //};

            lootDrop.Id = Guid.NewGuid().ToString();
            lootDrop.RegistryType = _lootTypes
                    .OrderBy(_ => _random.Next())
                    .First();

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
                        _logger.Error("Infinite loop situation here. Go fix it!");
                    }

                    effects.Add(effect);
                }

                if (lootDrop is IHasEffects lootWithEffects)
                {
                    lootWithEffects.EffectTypes = effects.ToList();
                }

                if (lootDrop is IHasTargetingAndShape consumer)
                {
                    consumer.TargetingType = GetRandomTargetingOrNone();
                    consumer.ShapeType = GetRandomShapeOrNone();
                }
            }

            var typeTranslation = _localizer.Translate("crafting.loot.type");
            var suffix = lootDrop is CombatItemBase combatItem
                ? int.Parse(combatItem.GetNameHash().ToString().TrimStart('-').Substring(5)).ToString("D5")
                : null;

            lootDrop.Name = $"{_localizer.Translate(lootDrop.RegistryType)} ({typeTranslation} #{suffix})";

            return lootDrop;
        }

        public ItemBase GetAmmoDrop()
        {
            var randomAmmo = _ammoTypes
                .OrderBy(_ => _random.Next())
                .First();

            var randomCount = _random.Next(randomAmmo.MinDropCount, randomAmmo.MaxDropCount);

            var item = _typeRegistry.GetItemTypeMapping<IAmmunitionType>().CreateNew();

            item.Id = Guid.NewGuid().ToString();
            item.RegistryType = randomAmmo;
            item.Name = _localizer.Translate(randomAmmo);
            item.Properties = new Dictionary<string, string> { { nameof(ItemStackBase.Count), randomCount.ToString() } };

            return item;
        }

        private int GetMinBiasedNumber(int min, int max)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(_random.NextDouble(), 3), 0);
        }

        private string GetItemNamePrefix(bool isAttack)
        {
            return _localizer.Translate(TranslationType.CraftingNamePrefix, isAttack ? "attack" : "defence");
        }

        public void SetItemName(ItemBase item, bool isAttack, string customSuffix = null)
        {
            var suffix = string.IsNullOrWhiteSpace(customSuffix)
                ? _localizer.Translate(item.RegistryType)
                : customSuffix;
            item.Name = $"{GetItemNamePrefix(isAttack)} {item.Attributes.GetIntValue(Strength.TypeId)} {suffix}";
        }

        public ItemBase GetCraftedItem(Type craftableType, string typeId, string resourceTypeId, bool isTwoHanded, string itemName, IList<ItemBase> components)
        {
            var properties = new Dictionary<string, string>
            {
                { IResourceConsumer.PropertyKeyResourceTypeId, resourceTypeId  },
                { nameof(CombatItemBase.IsTwoHanded), isTwoHanded.ToString() }
            };

            var craftedItem = _typeRegistry
                .GetCraftableItemTypeMapping(craftableType)
                .GetItemFromCrafting(components, typeId, itemName, properties);

            if (craftedItem == null)
            {
                throw new Exception($"Unexpected craftable category '{craftableType}'");
            }

            craftedItem.Id = Guid.NewGuid().ToString();
            craftedItem.RegistryType ??= _typeRegistry.GetAnyRegisteredTypeById(typeId);

            return craftedItem;
        }
    }
}
