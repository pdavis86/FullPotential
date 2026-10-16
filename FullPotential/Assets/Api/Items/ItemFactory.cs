using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Gameplay.Attributes;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.LivingEntity;

using Range = FullPotential.Api.Gameplay.Attributes.Range;

namespace FullPotential.Api.Items
{
    public class ItemFactory : IItemFactory
    {
        private readonly IAuditor _logger;
        private readonly ITypeRegistry _typeRegistry;

        private List<string> _lootTypeIds;
        private List<string> _accessoryTypeIds;
        private List<string> _armorTypeIds;
        private List<string> _weaponTypeIds;
        private List<string> _ammunitionTypeIds;
        private List<string> _specialTypeIds;

        public ItemFactory(IAuditorFactory auditorFactory, ITypeRegistry typeRegistry)
        {
            _logger = auditorFactory.Create(this);
            _typeRegistry = typeRegistry;
        }

        public ItemBase GetItemFromData(ItemData model)
        {
            ItemBase item;

            _lootTypeIds ??= _typeRegistry.GetRegisteredTypes<ILootType>().Select(x => x.TypeId.ToString()).ToList();
            _accessoryTypeIds ??= _typeRegistry.GetRegisteredTypes<IAccessoryType>().Select(x => x.TypeId.ToString()).ToList();
            _armorTypeIds ??= _typeRegistry.GetRegisteredTypes<IArmorType>().Select(x => x.TypeId.ToString()).ToList();
            _weaponTypeIds ??= _typeRegistry.GetRegisteredTypes<IWeaponType>().Select(x => x.TypeId.ToString()).ToList();
            _ammunitionTypeIds ??= _typeRegistry.GetRegisteredTypes<IAmmunitionType>().Select(x => x.TypeId.ToString()).ToList();
            _specialTypeIds ??= _typeRegistry.GetRegisteredTypes<ISpecialGearType>().Select(x => x.TypeId.ToString()).ToList();

            if (_lootTypeIds.Contains(model.RegistryTypeId))
            {
                item = _typeRegistry.GetItemTypeMapping<ILootType>().CreateNew();
            }
            else if (_accessoryTypeIds.Contains(model.RegistryTypeId))
            {
                item = _typeRegistry.GetItemTypeMapping<IAccessoryType>().CreateNew();
            }
            else if (_armorTypeIds.Contains(model.RegistryTypeId))
            {
                item = _typeRegistry.GetItemTypeMapping<IArmorType>().CreateNew();
            }
            else if (_weaponTypeIds.Contains(model.RegistryTypeId))
            {
                item = _typeRegistry.GetItemTypeMapping<IWeaponType>().CreateNew();
            }
            else if (_ammunitionTypeIds.Contains(model.RegistryTypeId))
            {
                item = _typeRegistry.GetItemTypeMapping<IAmmunitionType>().CreateNew();
            }
            else if (_specialTypeIds.Contains(model.RegistryTypeId))
            {
                item = _typeRegistry.GetItemTypeMapping<ISpecialGearType>().CreateNew();
            }
            else if (model.Properties.GetStringValueOrNull(IResourceConsumer.PropertyKeyResourceTypeId) != null)
            {
                item = _typeRegistry.GetItemTypeMapping<IConsumerType>().CreateNew();
            }
            else
            {
                throw new Exception("Unexpected type");
            }

            item.Id = model.Id;
            item.CharacterId = model.CharacterId;
            item.Name = model.Name;

            FillAttributeDictionary(model, item);
            FillPropertyDictionary(model, item);
            FillTypesFromIds(model, item);

            return item;
        }

        private void FillAttributeDictionary(ItemData model, ItemBase item)
        {
            if (model.Attributes == null)
            {
                item.Attributes = new Dictionary<string, string>();
                return;
            }

            item.Attributes = new Dictionary<string, string>();

            // todo: zzz v0.8 - Remove backwards compatibility: item.Attributes = new Dictionary<string, string>(model.Attributes);
            foreach (var kvp in model.Attributes)
            {
                var adjustedKey = kvp.Key switch
                {
                    "Strength" => Strength.TypeIdString,
                    "Efficiency" => Efficiency.TypeIdString,
                    "Range" => Range.TypeIdString,
                    "Accuracy" => Accuracy.TypeIdString,
                    "Speed" => Speed.TypeIdString,
                    "Recovery" => Recovery.TypeIdString,
                    "Duration" => Duration.TypeIdString,
                    "Luck" => Luck.TypeIdString,
                    _ => kvp.Key,
                };

                if (adjustedKey is "IsAutomatic" or "ExtraAmmoPerShot")
                {
                    continue;
                }

                item.Attributes[adjustedKey] = kvp.Value;
            }
        }

        private void FillPropertyDictionary(ItemData model, ItemBase item)
        {
            if (model.Properties == null)
            {
                item.Properties = new Dictionary<string, string>();
                return;
            }

            item.Properties = new Dictionary<string, string>(model.Properties);

            // todo: zzz v0.8 - Remove backwards compatibility
            item.Properties["IsAutomatic"] = model.Attributes?.GetStringValueOrNull("IsAutomatic");
            item.Attributes.Remove("IsAutomatic");
            item.Properties["ExtraAmmoPerShot"] = model.Attributes?.GetStringValueOrNull("ExtraAmmoPerShot");
            item.Attributes.Remove("ExtraAmmoPerShot");
        }

        public ItemData GetDataFromItem(string characterId, ItemBase item)
        {
            var model = new ItemData
            {
                Id = item.Id,
                CharacterId = characterId,
                RegistryTypeId = item.RegistryType?.TypeId.ToString(),
                Name = item.Name,
                Attributes = GetNonDefaultValues(item.Attributes),
                Properties = GetNonDefaultValues(item.Properties),
            };

            if (item is IHasEffects itemWithEffects)
            {
                model.EffectIds = itemWithEffects.EffectTypes.Select(t => t.TypeId.ToString()).ToList();
            }

            return model;
        }

        private Dictionary<string, string> GetNonDefaultValues(Dictionary<string, string> input)
        {
            if (input == null)
            {
                return new Dictionary<string, string>();
            }

            var nonDefaultAttributes = input.Where(
                kvp => !string.IsNullOrWhiteSpace(kvp.Value)
                && kvp.Value != "0"
                && !kvp.Value.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase));

            return nonDefaultAttributes.ToDictionary(x => x.Key, x => x.Value);
        }

        public void FillTypesFromIds(ItemData model, ItemBase item)
        {
            if (!string.IsNullOrWhiteSpace(model.RegistryTypeId) && item.RegistryType == null)
            {
                var itemType = _typeRegistry.GetAnyRegisteredTypeById(model.RegistryTypeId);

                if (itemType == null)
                {
                    _logger.Error("No registry type found for '{0}'", item.GetType().Name);
                    return;
                }

                item.RegistryType = itemType;
            }

            if (item is IHasTargetingAndShape withTargetingAndShape)
            {
                var targetingTypeId = item.Properties.GetStringValueOrNull(IHasTargetingAndShape.PropertyKeyTargetingTypeId);
                if (!string.IsNullOrWhiteSpace(targetingTypeId))
                {
                    withTargetingAndShape.TargetingType = _typeRegistry.GetRegisteredTypes<ITargetingType>()
                        .First(x => x.TypeId.ToString() == targetingTypeId);

                    withTargetingAndShape.TargetingVisualsType = _typeRegistry.GetRegisteredTypes<IItemVisualsType>()
                        .FirstOrDefault(v => v.TypeId.ToString() == item.Properties.GetStringValueOrNull(IHasTargetingAndShape.PropertyKeyTargetingVisualsTypeId));
                }

                var shapeTypeId = item.Properties.GetStringValueOrNull(IHasTargetingAndShape.PropertyKeyShapeTypeId);
                if (!string.IsNullOrWhiteSpace(shapeTypeId))
                {
                    withTargetingAndShape.ShapeType = _typeRegistry.GetRegisteredTypes<IShapeType>()
                        .First(x => x.TypeId.ToString() == shapeTypeId);

                    withTargetingAndShape.ShapeVisualsType = _typeRegistry.GetRegisteredTypes<IItemVisualsType>()
                        .FirstOrDefault(v => v.TypeId.ToString() == item.Properties.GetStringValueOrNull(IHasTargetingAndShape.PropertyKeyShapeVisualsTypeId));
                }
            }

            if (item is IResourceConsumer consumer)
            {
                consumer.ResourceType = _typeRegistry.GetRegisteredTypes<IResourceType>()
                    .First(x => x.TypeId.ToString() == item.Properties[IResourceConsumer.PropertyKeyResourceTypeId]);
            }

            if (item is IHasItemVisuals itemWithVisuals)
            {
                // todo: zzz v0.9 - Remove backwards compatibility
                var visualsTypeId = item.Properties.GetStringValueOrNull("AccessoryVisualsTypeId")
                    ?? item.Properties.GetStringValueOrNull("ArmorVisualsTypeId")
                    ?? item.Properties.GetStringValueOrNull("CustomVisualsTypeId")
                    ?? item.Properties.GetStringValueOrNull("WeaponVisualsTypeId");

                if (!string.IsNullOrWhiteSpace(visualsTypeId))
                {
                    // todo: surely I should match on the visualsTypeId?!
                    itemWithVisuals.Visuals = _typeRegistry.GetRegisteredTypes<IItemVisualsType>()
                        .FirstOrDefault(v => v.ApplicableToTypeIdString == item.RegistryType.TypeId.ToString());
                }
                else if (item.RegistryType != null)
                {
                    itemWithVisuals.Visuals = _typeRegistry.GetRegisteredTypes<IItemVisualsType>()
                        .FirstOrDefault(v => v.ApplicableToTypeIdString == item.RegistryType.TypeId.ToString());
                }
            }

            if (model.EffectIds != null && item is IHasEffects itemWithEffects)
            {
                itemWithEffects.EffectTypes = model.EffectIds
                    .Select(x => _typeRegistry.GetRegisteredByTypeId<IEffectType>(x))
                    .Where(x => x != null)
                    .ToList();
            }
        }
    }
}
