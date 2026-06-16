using System;
using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Obsolete.Items.Base;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Registry.Weapons;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Models.Player;

using UnityEngine;

namespace FullPotential.Api.Items
{
    public class ItemFactory : IItemFactory
    {
        private ITypeRegistry _typeRegistry;

        private List<string> _lootTypeIds;
        private List<string> _accessoryTypeIds;
        private List<string> _armorTypeIds;
        private List<string> _weaponTypeIds;
        private List<string> _ammunitionTypeIds;
        private List<string> _specialTypeIds;

        public ItemFactory(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public ItemBase GetItemFromData(ItemData model)
        {
            ItemBase item;

            // todo: zzz v0.6 - why are the models obsolete but the interfaces are not???
            _lootTypeIds ??= _typeRegistry.GetRegisteredTypes<ILootType>().Select(x => x.TypeId.ToString()).ToList();
            _accessoryTypeIds ??= _typeRegistry.GetRegisteredTypes<IAccessoryType>().Select(x => x.TypeId.ToString()).ToList();
            _armorTypeIds ??= _typeRegistry.GetRegisteredTypes<IArmorType>().Select(x => x.TypeId.ToString()).ToList();
            _weaponTypeIds ??= _typeRegistry.GetRegisteredTypes<IWeaponType>().Select(x => x.TypeId.ToString()).ToList();
            _ammunitionTypeIds ??= _typeRegistry.GetRegisteredTypes<IAmmunitionType>().Select(x => x.TypeId.ToString()).ToList();
            _specialTypeIds ??= _typeRegistry.GetRegisteredTypes<ISpecialGearType>().Select(x => x.TypeId.ToString()).ToList();

            if (_lootTypeIds.Contains(model.RegistryTypeId))
            {
                item = new Loot
                {
                    Id = model.Id,
                    CharacterId = model.CharacterId,
                    RegistryTypeId = model.RegistryTypeId,
                    Name = model.Name,
                    Attributes = GetAttributes(model.Attributes),
                    EffectIds = model.EffectIds.Select(x => x).ToArray(),
                    Effects = GetEffects(model),
                    TargetingTypeId = GetStringProperty(model, nameof(ItemWithTargetingAndShapeBase.TargetingTypeId)),
                    TargetingVisualsTypeId = GetStringProperty(model, nameof(ItemWithTargetingAndShapeBase.TargetingVisualsTypeId)),
                    ShapeTypeId = GetStringProperty(model, nameof(ItemWithTargetingAndShapeBase.ShapeTypeId)),
                    ShapeVisualsTypeId = GetStringProperty(model, nameof(ItemWithTargetingAndShapeBase.ShapeVisualsTypeId))
                };
            }
            else if (_accessoryTypeIds.Contains(model.RegistryTypeId))
            {
                item = new Accessory
                {
                    Id = model.Id,
                    CharacterId = model.CharacterId,
                    RegistryTypeId = model.RegistryTypeId,
                    Name = model.Name,
                    Attributes = GetAttributes(model.Attributes),
                    EffectIds = model.EffectIds.Select(x => x).ToArray(),
                    Effects = GetEffects(model),
                    AccessoryVisualsTypeId = GetStringProperty(model, nameof(Accessory.AccessoryVisualsTypeId))
                };
            }
            else if (_armorTypeIds.Contains(model.RegistryTypeId))
            {
                item = new Armor
                {
                    Id = model.Id,
                    CharacterId = model.CharacterId,
                    RegistryTypeId = model.RegistryTypeId,
                    Name = model.Name,
                    Attributes = GetAttributes(model.Attributes),
                    EffectIds = model.EffectIds.Select(x => x).ToArray(),
                    Effects = GetEffects(model),
                    ArmorVisualsTypeId = GetStringProperty(model, nameof(Armor.ArmorVisualsTypeId))
                };
            }
            else if (_weaponTypeIds.Contains(model.RegistryTypeId))
            {
                item = new Weapon
                {
                    Id = model.Id,
                    CharacterId = model.CharacterId,
                    RegistryTypeId = model.RegistryTypeId,
                    Name = model.Name,
                    Attributes = GetAttributes(model.Attributes),
                    EffectIds = model.EffectIds.Select(x => x).ToArray(),
                    Effects = GetEffects(model),
                    WeaponVisualsTypeId = GetStringProperty(model, nameof(Weapon.WeaponVisualsTypeId)),
                    Ammo = GetIntProperty(model, nameof(Weapon.Ammo))
                };
            }
            else if (_ammunitionTypeIds.Contains(model.RegistryTypeId))
            {
                item = new ItemStackBase
                {
                    Id = model.Id,
                    CharacterId = model.CharacterId,
                    RegistryTypeId = model.RegistryTypeId,
                    Name = model.Name,
                    Count = GetIntProperty(model, nameof(ItemStackBase.Count)),
                    BaseName = GetStringProperty(model, nameof(ItemStackBase.BaseName))
                };
            }
            else if (_specialTypeIds.Contains(model.RegistryTypeId))
            {
                item = new SpecialGear
                {
                    Id = model.Id,
                    CharacterId = model.CharacterId,
                    RegistryTypeId = model.RegistryTypeId,
                    Name = model.Name,
                    Attributes = GetAttributes(model.Attributes),
                    EffectIds = model.EffectIds.Select(x => x).ToArray(),
                    Effects = GetEffects(model),
                    IsTwoHanded = GetBoolProperty(model, nameof(SpecialGear.IsTwoHanded)),
                    CustomVisualsTypeId = GetStringProperty(model, nameof(SpecialGear.CustomVisualsTypeId)),
                    ResourceTypeId = GetStringProperty(model, nameof(SpecialGear.ResourceTypeId)),
                    CustomData = model.Properties
                        .Where(kvp => kvp.Key != nameof(SpecialGear.CustomVisualsTypeId) && kvp.Key != nameof(SpecialGear.ResourceTypeId))
                        .Select(kvp => new SerializableKeyValuePair<string, string>(kvp.Key, kvp.Value))
                        .ToArray()
                };
            }
            else if (model.Properties.GetStringValueOrNull(nameof(Consumer.ResourceTypeId)) != null)
            {
                item = new Consumer
                {
                    Id = model.Id,
                    CharacterId = model.CharacterId,
                    RegistryTypeId = model.RegistryTypeId,
                    Name = model.Name,
                    Attributes = GetAttributes(model.Attributes),
                    EffectIds = model.EffectIds.Select(x => x).ToArray(),
                    Effects = GetEffects(model),
                    IsTwoHanded = GetBoolProperty(model, nameof(Consumer.IsTwoHanded)),
                    TargetingTypeId = GetStringProperty(model, nameof(Consumer.TargetingTypeId)),
                    TargetingVisualsTypeId = GetStringProperty(model, nameof(Consumer.TargetingVisualsTypeId)),
                    ShapeTypeId = GetStringProperty(model, nameof(Consumer.ShapeTypeId)),
                    ShapeVisualsTypeId = GetStringProperty(model, nameof(Consumer.ShapeVisualsTypeId)),
                    ResourceTypeId = GetStringProperty(model, nameof(Consumer.ResourceTypeId))
                };
            }
            else
            {
                throw new Exception("Unexpected type");
            }

            FillAttributeDictionary(model, item);
            FillPropertyDictionary(model, item);
            FillTypesFromIds(item);

            return item;
        }

        private void FillAttributeDictionary(ItemData model, ItemBase item)
        {
            if (model.Attributes == null)
            {
                return;
            }

            foreach (var kvp in model.Attributes)
            {
                item.AttributeDictionary[kvp.Key] = kvp.Value;
            }
        }

        private void FillPropertyDictionary(ItemData model, ItemBase item)
        {
            foreach (var kvp in model.Properties)
            {
                item.PropertyDictionary[kvp.Key] = kvp.Value;
            }
        }

        private Attributes GetAttributes(Dictionary<string, string> attributes)
        {
            return new Attributes
            {
                IsAutomatic = attributes.ContainsKey(nameof(Attributes.IsAutomatic)) ? bool.Parse(attributes[nameof(Attributes.IsAutomatic)]) : false,
                ExtraAmmoPerShot = attributes.ContainsKey(nameof(Attributes.ExtraAmmoPerShot)) ? byte.Parse(attributes[nameof(Attributes.ExtraAmmoPerShot)]) : (byte)0,
                Strength = attributes.ContainsKey(nameof(Attributes.Strength)) ? int.Parse(attributes[nameof(Attributes.Strength)]) : 0,
                Efficiency = attributes.ContainsKey(nameof(Attributes.Efficiency)) ? int.Parse(attributes[nameof(Attributes.Efficiency)]) : 0,
                Range = attributes.ContainsKey(nameof(Attributes.Range)) ? int.Parse(attributes[nameof(Attributes.Range)]) : 0,
                Accuracy = attributes.ContainsKey(nameof(Attributes.Accuracy)) ? int.Parse(attributes[nameof(Attributes.Accuracy)]) : 0,
                Speed = attributes.ContainsKey(nameof(Attributes.Speed)) ? int.Parse(attributes[nameof(Attributes.Speed)]) : 0,
                Recovery = attributes.ContainsKey(nameof(Attributes.Recovery)) ? int.Parse(attributes[nameof(Attributes.Recovery)]) : 0,
                Duration = attributes.ContainsKey(nameof(Attributes.Duration)) ? int.Parse(attributes[nameof(Attributes.Duration)]) : 0,
                Luck = attributes.ContainsKey(nameof(Attributes.Luck)) ? int.Parse(attributes[nameof(Attributes.Luck)]) : 0
            };
        }

        private Dictionary<string, string> GetAttributeDictionary(Attributes attributes)
        {
            return new Dictionary<string, string>
            {
                { nameof(Attributes.IsAutomatic), attributes.IsAutomatic.ToString() },
                { nameof(Attributes.ExtraAmmoPerShot), attributes.ExtraAmmoPerShot.ToString() },
                { nameof(Attributes.Strength), attributes.Strength.ToString() },
                { nameof(Attributes.Efficiency), attributes.Efficiency.ToString() },
                { nameof(Attributes.Range), attributes.Range.ToString() },
                { nameof(Attributes.Accuracy), attributes.Accuracy.ToString() },
                { nameof(Attributes.Speed), attributes.Speed.ToString() },
                { nameof(Attributes.Recovery), attributes.Recovery.ToString() },
                { nameof(Attributes.Duration), attributes.Duration.ToString() },
                { nameof(Attributes.Luck), attributes.Luck.ToString() },
            };
        }

        private bool GetBoolProperty(ItemData model, string key)
        {
            return model.Properties.ContainsKey(key) ? bool.Parse(model.Properties[key]) : false;
        }

        private int GetIntProperty(ItemData model, string key)
        {
            return model.Properties.ContainsKey(key) ? int.Parse(model.Properties[key]) : 0;
        }

        private string GetStringProperty(ItemData model, string key)
        {
            return model.Properties.ContainsKey(key) ? model.Properties[key] : null;
        }

        public ItemData GetDataFromItem(string characterId, ItemBase item)
        {
            var itemData = new ItemData
            {
                Id = item.Id,
                CharacterId = characterId,
                RegistryTypeId = item.RegistryTypeId,
                Name = item.Name
            };

            if (item is CombatItemBase combatItem)
            {
                itemData.Attributes = GetAttributeDictionary(combatItem.Attributes);
                itemData.EffectIds = combatItem.EffectIds?.Select(x => x).ToList() ?? new List<string>();
                itemData.Properties = new Dictionary<string, string>
                {
                    { nameof(CombatItemBase.IsTwoHanded), combatItem.IsTwoHanded.ToString() },
                };
            }

            if (item is ItemWithTargetingAndShapeBase complexItem)
            {
                itemData.Properties[nameof(ItemWithTargetingAndShapeBase.TargetingTypeId)] = complexItem.TargetingTypeId;
                itemData.Properties[nameof(ItemWithTargetingAndShapeBase.TargetingVisualsTypeId)] = complexItem.TargetingVisualsTypeId;
                itemData.Properties[nameof(ItemWithTargetingAndShapeBase.ShapeTypeId)] = complexItem.ShapeTypeId;
                itemData.Properties[nameof(ItemWithTargetingAndShapeBase.ShapeVisualsTypeId)] = complexItem.ShapeVisualsTypeId;
            }

            // todo: zzz v0.6 - why are there separate xxxVisualsTypeId for each type?
            switch (item)
            {
                case Accessory accessory:
                    itemData.Properties[nameof(Accessory.AccessoryVisualsTypeId)] = accessory.AccessoryVisualsTypeId;
                    break;

                case Armor armor:
                    itemData.Properties[nameof(Armor.ArmorVisualsTypeId)] = armor.ArmorVisualsTypeId;
                    break;

                case Weapon weapon:
                    itemData.Properties[nameof(Weapon.WeaponVisualsTypeId)] = weapon.WeaponVisualsTypeId;
                    itemData.Properties[nameof(Weapon.Ammo)] = weapon.Ammo.ToString();
                    break;

                case Consumer consumer:
                    itemData.Properties[nameof(Consumer.ResourceTypeId)] = consumer.ResourceTypeId;
                    break;

                case ItemStackBase itemStack:
                    // todo: zzz v0.6 - sort out this BaseName nonsense
                    itemData.Properties = new Dictionary<string, string>
                    {
                        { nameof(ItemStackBase.Count) , itemStack.CountForSerialization.ToString() },
                        { nameof(ItemStackBase.BaseName), itemStack.BaseName }
                    };
                    break;

                case SpecialGear specialGear:
                    itemData.Properties[nameof(SpecialGear.CustomVisualsTypeId)] = specialGear.CustomVisualsTypeId;
                    itemData.Properties[nameof(SpecialGear.ResourceTypeId)] = specialGear.ResourceTypeId;
                    foreach (var kvp in specialGear.CustomData)
                    {
                        itemData.Properties[kvp.Key] = kvp.Value;
                    }
                    break;
            }

            return itemData;
        }

        public void FillTypesFromIds(ItemBase item)
        {
            if (!string.IsNullOrWhiteSpace(item.RegistryTypeId) && item.RegistryType == null)
            {
                var itemType = _typeRegistry.GetRegistryTypeForItem(item);

                if (itemType == null)
                {
                    Debug.LogError($"No registry type found for '{item.GetType().Name}'");
                    return;
                }

                item.RegistryType = itemType;
            }

            if (item is ItemWithTargetingAndShapeBase withTargetingAndShape && !string.IsNullOrWhiteSpace(withTargetingAndShape.TargetingTypeId))
            {
                withTargetingAndShape.Targeting = _typeRegistry.GetRegisteredTypes<ITargetingType>()
                    .First(x => x.TypeId.ToString() == withTargetingAndShape.TargetingTypeId);

                withTargetingAndShape.TargetingVisuals = _typeRegistry.GetRegisteredTypes<ITargetingVisuals>()
                    .FirstOrDefault(v => v.TypeId.ToString() == withTargetingAndShape.TargetingVisualsTypeId);

                if (!string.IsNullOrWhiteSpace(withTargetingAndShape.ShapeTypeId))
                {
                    withTargetingAndShape.Shape = _typeRegistry.GetRegisteredTypes<IShapeType>()
                        .First(x => x.TypeId.ToString() == withTargetingAndShape.ShapeTypeId);

                    withTargetingAndShape.ShapeVisuals = _typeRegistry.GetRegisteredTypes<IShapeVisuals>()
                        .FirstOrDefault(v => v.TypeId.ToString() == withTargetingAndShape.ShapeVisualsTypeId);
                }
            }

            if (item is Consumer consumer)
            {
                consumer.ResourceType = _typeRegistry.GetRegisteredTypes<IResourceType>()
                    .First(x => x.TypeId.ToString() == consumer.ResourceTypeId);
            }
            else if (item is SpecialGear specialGear)
            {
                specialGear.ResourceType = _typeRegistry.GetRegisteredTypes<IResourceType>()
                    .First(x => x.TypeId.ToString() == specialGear.ResourceTypeId);
            }

            if (item is IHasItemVisuals itemWithVisuals)
            {
                switch (item)
                {
                    case Weapon:
                        SetItemVisuals<IWeaponVisuals>(itemWithVisuals, item);
                        break;

                    case Armor:
                        SetItemVisuals<IArmorVisuals>(itemWithVisuals, item);
                        break;

                    case Accessory:
                        SetItemVisuals<IAccessoryVisuals>(itemWithVisuals, item);
                        break;

                    case SpecialGear:
                        SetItemVisuals<ISpecialGearVisuals>(itemWithVisuals, item);
                        break;
                }
            }

            if (item is CombatItemBase combatItem)
            {
                if (combatItem.EffectIds != null && combatItem.EffectIds.Length > 0 && combatItem.Effects == null)
                {
                    combatItem.Effects = combatItem.EffectIds
                        .Select(x => _typeRegistry.GetRegisteredByTypeId<IEffectType>(x))
                        .Where(x => x != null)
                        .ToList();
                }

                //For backwards compatibility
                combatItem.Effects ??= new List<IEffectType>();
                if (!combatItem.Effects.Any())
                {
                    combatItem.Effects.Add(_typeRegistry.GetRegisteredByTypeId<IEffectType>(EffectTypeIds.HurtId));
                }
            }
        }

        private void SetItemVisuals<T>(IHasItemVisuals itemWithVisuals, ItemBase item)
            where T : IItemVisuals
        {
            if (itemWithVisuals.VisualsTypeId.IsNullOrWhiteSpace())
            {
                itemWithVisuals.Visuals = _typeRegistry.GetRegisteredTypes<T>()
                    .FirstOrDefault(v => v.ApplicableToTypeIdString == item.RegistryType.TypeId.ToString());
            }
            else
            {
                itemWithVisuals.Visuals = _typeRegistry.GetRegisteredTypes<T>()
                    .FirstOrDefault(v => v.TypeId.ToString() == itemWithVisuals.VisualsTypeId);
            }
        }

        private List<IEffectType> GetEffects(ItemData model)
        {
            return model.EffectIds
                .Select(x => _typeRegistry.GetRegisteredByTypeId<IEffectType>(x))
                .Where(x => x != null)
                .ToList();
        }
    }
}
